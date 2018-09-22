using System.ComponentModel.DataAnnotations.Schema;

namespace Telimena.WebApp.Core.Models
{
    public class ProgramCustomUsageData
    {
        protected ProgramCustomUsageData() { }

        public ProgramCustomUsageData(string data)
        {
            this.Data = data;
        }
        [ForeignKey(nameof(ProgramUsageDetail))]
        public int Id { get; set; }

        public string Data { get; set; }
        public ProgramUsageDetail ProgramUsageDetail { get; set; }
    }
}