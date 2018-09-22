using System.ComponentModel.DataAnnotations.Schema;

namespace Telimena.WebApp.Core.Models
{
    public class FunctionCustomUsageData 
    {
        protected FunctionCustomUsageData() { }

        public FunctionCustomUsageData(string data)
        {
            this.Data = data;
        }
        [ForeignKey(nameof(FunctionUsageDetail))]
        public int Id { get; set; }

        public string Data { get; set; }
        public FunctionUsageDetail FunctionUsageDetail { get; set; }
    }
}