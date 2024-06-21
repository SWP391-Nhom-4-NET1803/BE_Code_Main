using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RepoClinicService = Repositories.Models.ClinicService;

namespace Services.ClinicsService
{
    public interface IClinicsService: IDisposable
    {
        bool CreateClinicService(ClinicService service);

        bool CreateClinic(ClinicRegistrationModel clinc, out string message);

        Clinic? GetClinicInformation(int clinicId);

        public ClinicService GetServiceInfo(Guid serviceId);

        IEnumerable<RepoClinicService> GetClinicServices(int clinicId);

        bool UpdateClinicInformation(Clinic clinic, out string message);

    }
}
