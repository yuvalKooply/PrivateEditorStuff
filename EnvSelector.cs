
using System.IO;
using Com.Kooply.Unity;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace Editor.Private
{
#if UNITY_EDITOR

    public static class EnvSelector
    {
        private const string DevEnvPath = "Kooply/Environment/Dev";
        private const string StageEnvPath = "Kooply/Environment/Stage";
        private const string ProdEnvPath = "Kooply/Environment/Prod";
    
    
        static EnvSelector()
        {
            RefreshSelectedEnv();
        }
        
        private static void RefreshSelectedEnv(string env = null)
        {
            if (env == null)
            {
                var generalConfPath = "Assets/Resources/generalConfiguration-editor.json";
                var generalConf = JObject.Parse(File.ReadAllText(generalConfPath));
                env = generalConf.SelectToken("env")?.Value<string>();
            }

            Menu.SetChecked(DevEnvPath, env == Envs.Dev);
            Menu.SetChecked(StageEnvPath, env == Envs.Stage);
            Menu.SetChecked(ProdEnvPath, env == Envs.Prod);
        }
        
        [MenuItem(DevEnvPath)]
        public static void SetEnvDev()
        {
            SetEnv(Envs.Dev);
            RefreshSelectedEnv(Envs.Dev);
        }
  
        [MenuItem(StageEnvPath)]
        public static void SetEnvStage()
        {
            SetEnv(Envs.Stage);
            RefreshSelectedEnv(Envs.Stage);
        }
        
        [MenuItem(ProdEnvPath)]
        public static void SetEnvProd()
        {
            SetEnv(Envs.Prod);
            RefreshSelectedEnv(Envs.Prod);
        }

        private static void SetEnv(string env)
        {
            var generalConfPath = "Assets/Resources/generalConfiguration-editor.json";
            var generalConf = JObject.Parse(File.ReadAllText(generalConfPath));
            var envToken = generalConf.SelectToken("env");
            envToken?.Replace(env);
            File.WriteAllText(generalConfPath, generalConf.ToString());
        }
    }
    
#endif
}