using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;

namespace CKAN
{
    public class Plugin
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Plugin));

        public Plugin(string assemblyPath)
        {
            m_AssemblyPath = assemblyPath;
            m_AppDomain = AppDomain.CreateDomain(assemblyPath);
            m_Plugin = (IGUIPlugin)m_AppDomain.CreateInstanceFromAndUnwrap(assemblyPath, typeof(IGUIPlugin).FullName);
        }

        public void Activate()
        {
            if (m_Active)
            {
                return;
            }

            try
            {
                m_Plugin.Initialize();
                m_Active = true;
            }
            catch (Exception ex)
            {
                m_Active = false;
                log.ErrorFormat("Failed to activate plugin \"{0} - {1}\" - {2}", m_Plugin.GetName(), m_Plugin.GetVersion(), ex.Message);
            }
        }

        public void Deactivate()
        {
            if (!m_Active)
            {
                return;
            }

            try
            {
                m_Plugin.Deinitialize();
                m_Active = true;
            }
            catch (Exception ex)
            {
                m_Active = false;
                log.ErrorFormat("Failed to activate plugin \"{0} - {1}\" - {2}", m_Plugin.GetName(), m_Plugin.GetVersion(), ex.Message);
            }
        }

        public bool IsActive
        {
            get
            {
                return m_Active;
            }
        }
        
        private AppDomain m_AppDomain = null;
        private IGUIPlugin m_Plugin = null;
        private string m_AssemblyPath = null;
        private bool m_Active = false;

    }

    public class PluginController
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(PluginController));

        private string m_PluginsPath = "";

        public PluginController(string path, bool doActivate = true)
        {
            m_PluginsPath = path;

            foreach (string dll in Directory.GetFiles(path, "*.dll"))
            {
                m_Plugins.Add(new Plugin(dll));
            }
        }

        public void AddNewAssemblyToPluginsPath(string path)
        {
            if (Path.GetExtension(path) != ".dll")
            {
                log.ErrorFormat("Not a .dll, skipping..");
                return;
            }

            var targetPath = Path.Combine(m_PluginsPath, Path.GetFileName(path));
            if (File.Exists(targetPath))
            {
                try
                {
                    File.Delete(targetPath);
                }
                catch (Exception)
                {
                    log.ErrorFormat("Cannot copy plugin to {0}, because it already exists and is open..", targetPath);
                    return;
                }
            }

            File.Copy(path, targetPath);
            m_Plugins.Add(new Plugin(targetPath));
        }

        public void UnloadPlugin(Plugin plugin)
        {
            plugin.Deactivate();
            m_Plugins.Remove(plugin);
        }

        public List<Plugin> Plugins
        {
            get { return m_Plugins.ToList(); }
        }

        private HashSet<Plugin> m_Plugins = new HashSet<Plugin>();

    }

}
