using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Compilation;
using UnityEngine;

using System.IO;
using System.Xml;

class AddXboxLiveConfig : IPostprocessBuild {

    //private string ServiceConfigId = "00000000-0000-0000-0000-000000000000";
    //private string DecimalTitleId = "1234567890";

    private string ServiceConfigId = "SCID_GOES_HERE"; // replace this with your own scid
    private string DecimalTitleId = "TITLEID_GOES_HERE"; // replace this with your own title id

    private string BuildPath = "";

    public int callbackOrder { get { return 0; } }

    public void OnPostprocessBuild(BuildTarget target, string path)
    {
        Debug.Log("processor for target " + target + " at path " + path);
        Debug.Log("Player settings: " + PlayerSettings.productName);
        
        //if the build isn't for UWP, then just ignore
        if (target != BuildTarget.WSAPlayer)
            return;

        if (Directory.Exists(path))
        {
            Debug.Log("Dir exists " + path);

            string actualPath = path + "/" + PlayerSettings.productName;
            Debug.Log(actualPath);

            //Check for the actual project file directory to make sure it exists
            if (Directory.Exists(actualPath))
            {
                //If the directory exists, we can create the services config file
                Debug.Log("Dir exists " + actualPath + " creating config file");

                string xboxServicesFilePath = actualPath + "/xboxservices.config";

                //Create the xbox services config file, or overwrite the current one with changes
                try {
                    
                    Debug.Log("Creating xbox services config file : " + xboxServicesFilePath);

                    using (StreamWriter SWriter = File.CreateText(xboxServicesFilePath))
                    {
                        SWriter.WriteLine("{");
                        SWriter.WriteLine("\"TitleId\" : " + DecimalTitleId + ",");
                        SWriter.WriteLine("\"PrimaryServiceConfigId\" : \"" + ServiceConfigId + "\"");
                        SWriter.WriteLine("}");

                        SWriter.Close();
                    }

                    //Double check to make sure the file saved
                    if (File.Exists(xboxServicesFilePath))
                    {
                        Debug.Log("Created xbox services config file : " + xboxServicesFilePath);
                    }
                    
                }
                catch(System.IO.IOException e)
                {
                    Debug.Log("File IO Exception trying to create xboxservices.config");
                }

                //Start adding the config file to the VS project file
                string mainProjectFile = actualPath + "/" + PlayerSettings.productName + ".vcxproj";

                if (File.Exists(mainProjectFile))
                {
                    //Project file was found so now add the config file as content to the project file
                    Debug.Log("Project file found: " + mainProjectFile);

                    //Load the output project file
                    XmlDocument projectFile = new XmlDocument();
                    projectFile.Load(mainProjectFile);

                    // If the services config file already exists we don't need to do anything.
                    if (CheckForXboxConfig(projectFile))
                    {
                        Debug.Log("xboxservices.config already found in project");
                        return;
                    }

                    //Add the services file to the project
                    AddConfigFileToProject(projectFile, xboxServicesFilePath);

                    //Save the changes
                    projectFile.Save(mainProjectFile);

                    Debug.Log("Project file saved: " + mainProjectFile);
                }
            }
        }
    }

    private bool CheckForXboxConfig(XmlDocument document)
    {
        XmlNodeList elemList = document.GetElementsByTagName("None");
        for (int i = 0; i < elemList.Count; i++)
        {
            foreach (XmlAttribute attribute in elemList[i].Attributes)
            {
                Debug.Log("Attribute: " + attribute.Name + " " + attribute.Value);
                if (attribute.Name == "Include" && attribute.Value.Contains("xboxservices.config"))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void AddConfigFileToProject(XmlDocument projectFile, string configFilePath)
    {

        XmlNamespaceManager mgr = new XmlNamespaceManager(projectFile.NameTable);
        mgr.AddNamespace("x", "http://schemas.microsoft.com/developer/msbuild/2003");

        XmlNode firstCompileNode = projectFile.SelectSingleNode("/x:Project/x:ItemGroup", mgr);

        XmlElement configElement = projectFile.CreateElement("None");

        configElement.SetAttribute("Include", configFilePath);

        XmlElement deploymentContent = projectFile.CreateElement("DeploymentContent");
        deploymentContent.InnerText = "True";

        firstCompileNode.AppendChild(configElement);

        configElement.AppendChild(deploymentContent);

        return;
    }
}
