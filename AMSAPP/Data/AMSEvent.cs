
namespace AMSAPP
{
    using System;
    using System.Collections.Generic;
    
    public partial class AMSEvent
    {

        public AMSEvent()
        {
            IsActual = true;
        }

        public int Id { get; set; }
        public Nullable<byte> EventType { get; set; }
        public System.DateTime EventOn { get; set; }
        public string Description { get; set; }
        public bool IsActual { get; set; }
        public string ElapsedTimeTxt { get; set; }

        public Nullable<System.TimeSpan> ElapsedTime { 
            get
            {
                TimeSpan retVal = TimeSpan.Zero;

                try 
	            {
                    retVal = TimeSpan.Parse(this.ElapsedTimeTxt);
	            }
	            catch (Exception)
	            {
                    retVal = TimeSpan.Zero;
	            }

                return retVal;
            }
            set
            {
                if (value == null)
                {
                    this.ElapsedTimeTxt = "";
                }
                else
                {
                    this.ElapsedTimeTxt = value.ToString();
                }
            }
        }
    }
}
