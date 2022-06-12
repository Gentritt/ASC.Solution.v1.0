using ASC.Web.Models.AccountViewModels;
using System.Collections.Generic;

namespace ASC.Solution.v1._1.Models.AccountViewModels
{
    public class ServiceEngineerRegistrationViewModel : RegisterViewModel
    {
        public string UserName { get; set; }
        public bool IsEdit { get; set; }
        public bool IsActive { get; set; }

    }

    public class ServiceEngineerViewModel
    {
        public List<ApplicationUser> ServiceEngineers { get; set; }
        public ServiceEngineerRegistrationViewModel Registration { get; set; }
    }
}
