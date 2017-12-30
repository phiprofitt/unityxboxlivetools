using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

//this class stores the xbox live config
public static class XboxLiveConfig
{
    private static string ServiceConfigId = "SCID_GOES_HERE"; // replace this with your own scid
    private static string DecimalTitleId = "TITLEID_GOES_HERE"; // replace this with your own title id

    public static string Scid
    {
        get { return ServiceConfigId; }
        set { ServiceConfigId = value; }
    }
    public static string TitleId
    {
        get { return DecimalTitleId; }
        set { DecimalTitleId = value; }
    }


    public static bool CheckValidGUID(string serviceConfigID)
    {

        string GUIDRegexPattern = @"([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12})";

        Regex rgx = new Regex(GUIDRegexPattern);
        bool isValidGuid = rgx.IsMatch(serviceConfigID);

        if (!isValidGuid)
            Debug.Log("! Error ! Invalid Service Config ID");

        return isValidGuid;
    }

    public static bool CheckValidTitleID(string titleID)
    {
        string titleIdRegexPattern = @"([0-9]{9})";

        Regex rgx = new Regex(titleIdRegexPattern);
        bool isValidTitleId = rgx.IsMatch(titleID);

        if (!isValidTitleId)
            Debug.Log("! Error ! Invalid Title ID - perhaps you aren't using the decimal ID?");

        return isValidTitleId;
    }

    public static bool CreateConfigFile(string path)
    {
        //Create the xbox services config file, or overwrite the current one with changes
        try
        {
            using (StreamWriter SWriter = File.CreateText(path))
            {
                SWriter.WriteLine("{");
                SWriter.WriteLine("\"TitleId\" : " + XboxLiveConfig.TitleId + ",");
                SWriter.WriteLine("\"PrimaryServiceConfigId\" : \"" + XboxLiveConfig.Scid + "\"");
                SWriter.WriteLine("}");

                SWriter.Close();
            }

            //Double check to make sure the file saved
            if (File.Exists(path))
            {
#if XBL_DEBUG
                Debug.Log("Created xbox services config file : " + path);
#endif
                return true;
            }

        }
        catch (System.IO.IOException e)
        {
            Debug.Log("File IO Exception trying to create xboxservices.config");
            return false;
        }

        return false;
    }
}

//This class creates a window to edit the xbox live configuration
[InitializeOnLoad]
public class XBLConfigWindow : EditorWindow
{
    static string scid = XboxLiveConfig.Scid;
    static string titleId = XboxLiveConfig.TitleId;
    static bool isLoaded = false;

    static XBLConfigWindow()
    {
        //Subscribe to update to load the config once assets are loaded
        EditorApplication.update += Update;
    }

    static void Update()
    {
        //On update check to see if we have loaded our IDs yet, if not try loading them
        if (!isLoaded)
            Load();
        else
            EditorApplication.update -= Update;
    }

    [MenuItem("UWP / Xbox Live/Configure IDs")]
    public static void ConfigureLiveIDs()
    {
        EditorWindow.GetWindow(typeof(XBLConfigWindow));
    }


    private void OnGUI()
    {
        GUILayout.Label("Xbox Live Configuration", EditorStyles.boldLabel);
        scid = EditorGUILayout.TextField("Service Config ID: ", scid);
        titleId = EditorGUILayout.TextField("Title ID: ", titleId);

        if (GUILayout.Button("Save"))
        {
            Save();
        }
        if (GUILayout.Button("Load"))
        {
            Load();
        }
    }

    private static void Save()
    {
        if (!XboxLiveConfig.CheckValidGUID(scid) || !XboxLiveConfig.CheckValidTitleID(titleId))
            return;
        else
        {
            Debug.Log("Saved successfully! SCID: " + XboxLiveConfig.Scid + " TitleID: " + XboxLiveConfig.TitleId);

            XboxLiveConfig.Scid = scid;
            XboxLiveConfig.TitleId = titleId;

            string folderpath = Application.dataPath + "/Editor/XboxLiveConfig";

            if (!Directory.Exists(folderpath))
            {
                Directory.CreateDirectory(folderpath);
            }

            if (!XboxLiveConfig.CreateConfigFile(folderpath + "xboxservices.config"))
                return;

            Debug.Log("Saved successfully! SCID: " + XboxLiveConfig.Scid + " TitleID: " + XboxLiveConfig.TitleId);
        }
    }

    private static void Load()
    {
        string folderpath = Application.dataPath + "/Editor/XboxLiveConfig";

        if (!Directory.Exists(folderpath))
            return;

        string filepath = folderpath + "/xboxservices.config";
        if (File.Exists(filepath))
        {
            try { 
                string[] lines = File.ReadAllLines(filepath);

                foreach (string line in lines)
                {
                    var scidMatch = Regex.Match(line, @"[{(]?[0-9A-F]{8}[-]?([0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?");
                    if (scidMatch.Success) { //scid found
                        scid = scidMatch.Value;
                        XboxLiveConfig.Scid = scid;
                        Debug.Log("XBLConfig loaded scid : " + scid);
                    }

                    var titleIdMatch = Regex.Match(line, @"([0-9]{10}),");
                    if (titleIdMatch.Success)
                    { //scid found
                        titleId = titleIdMatch.Groups[1].Value;
                        XboxLiveConfig.TitleId = titleId;
                        Debug.Log("XBLConfig loaded titleId : " + titleId);
                    }
                }
            }
            catch(System.IO.IOException e)
            {
                //no file exists so no loading needs to be done
            }
        }

        isLoaded = true;
    }
}

//add XBL_DEBUG define to Unity defines to enable debugging
class AddXboxLiveConfig : IPostprocessBuild {

    //private string ServiceConfigId = "00000000-0000-0000-0000-000000000000";
    //private string DecimalTitleId = "1234567890";
    
    

    public int callbackOrder { get { return 0; } }

    public void OnPostprocessBuild(BuildTarget target, string path)
    {
        
#if XBL_DEBUG
        Debug.Log("processor for target " + target + " at path " + path);
        Debug.Log("Player settings: " + PlayerSettings.productName);
#endif
        //if the build isn't for UWP, then just ignore
        if (target != BuildTarget.WSAPlayer)
            return;

        //Check for a valid GUID in the service config ID
        if (!XboxLiveConfig.CheckValidGUID(XboxLiveConfig.Scid))
            return;

        //Check for a valid decimal title ID
        if (!XboxLiveConfig.CheckValidTitleID(XboxLiveConfig.TitleId))
            return;

        if (Directory.Exists(path))
        {
            string actualPath = path + "/" + PlayerSettings.productName;

            //Check for the actual project file directory to make sure it exists
            if (Directory.Exists(actualPath))
            {
#if XBL_DEBUG
                //If the directory exists, we can create the services config file
                Debug.Log("Dir exists " + actualPath + " creating config file");
#endif
                string xboxServicesFilePath = actualPath + "/xboxservices.config";

                if(!XboxLiveConfig.CreateConfigFile(xboxServicesFilePath))
                    return;

                //Start adding the config file to the VS project file
                string mainProjectFile = actualPath + "/" + PlayerSettings.productName + ".vcxproj";

                if (File.Exists(mainProjectFile))
                {
#if XBL_DEBUG
                    //Project file was found so now add the config file as content to the project file
                    Debug.Log("Project file found: " + mainProjectFile);
#endif
                    //Load the output project file
                    XmlDocument projectFile = new XmlDocument();
                    projectFile.Load(mainProjectFile);

                    // If the services config file already exists we don't need to do anything.
                    if (CheckForXboxConfig(projectFile))
                    {
#if XBL_DEBUG
                        Debug.Log("xboxservices.config already found in project");
#endif
                        return;
                    }

                    //Add the services file to the project
                    AddConfigFileToProject(projectFile, xboxServicesFilePath);

                    //Save the changes
                    projectFile.Save(mainProjectFile);
#if XBL_DEBUG
                    Debug.Log("Project file saved: " + mainProjectFile);
#endif
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
