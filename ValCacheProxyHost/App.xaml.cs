using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.ServiceModel;
using ValCacheProxyLib;

namespace ValCacheProxyHost
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        ServiceHost m_host;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            m_host = new ServiceHost(typeof(FileServerProxyService));
            m_host.Open();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            m_host.Close();

            base.OnExit(e);
        }
    }
}
