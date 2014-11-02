using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThesisBackupper.ViewModels;

namespace ThesisBackupper
{
   public  class MainBootstrapper : BootstrapperBase
    {
       public MainBootstrapper()
       {
           Initialize();
       }

       protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
       {
           DisplayRootViewFor<MainViewModel>();
       }
    }
}
