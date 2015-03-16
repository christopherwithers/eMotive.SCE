using System.Collections.Generic;

namespace eMotive.Models.Objects.Account
{
    public class SCEFormData
    {
        public SCEFormData()
        {
            Grades = new Dictionary<string, string>
            {
                {"0","Not a SCE"},
                {"1","Consultant"},
                {"2","GP"},
                {"3","Associate Specialist"},
                {"4","Specialty Doctor"},
                {"5","Staff Grade"},
                {"6","Specialist Registrar"},
                {"7","Clinical Teaching Fellow"},
                {"8","Foundation Year 2"},
            };

            Trusts = new Dictionary<string, string>
            {
                {"0","Not applicable"},
                {"1","Birmingham and Solihull Mental Health NHS Foundation Trust"},
                {"2","Birmingham Children's Hospital NHS Foundation Trust"},
                {"3","Birmingham Women's NHS Foundation Trust"},
                {"4","Black Country Partnership NHS Foundation Trust"},
                {"5","Dudley & Walsall Mental Health Partnership NHS Trust"},
                {"6","GP"},
                {"7","Heart of England NHS Foundation Trust"},
                {"8","Royal Orthopaedic Hospital NHS Foundation Trust"},
                {"9","Sandwell & West Birmingham Hospitals NHS Trust"},
                {"10","The Dudley Group of Hospitals NHS Trust "},
                {"11","The Royal Wolverhampton Hospitals NHS Trust"},
                {"12","UHB NHS Foundation Trust"},
                {"13","Walsall Healthcare NHS Trust"},
                {"14","Worcestershire Acute Hospitals NHS Trust"},
                {"15","Wye Valley NHS Trust"},
                {"16", "South Birmingham Primary Care Trust"},
                {"17", "Wolverhampton City Primary Care Trust"},
                {"18", "Worcestershire Primary Care Trust"}
            };
        }

        public Dictionary<string, string> Trusts { get; set; }
        public Dictionary<string, string> Grades { get; set; }
    }
}
