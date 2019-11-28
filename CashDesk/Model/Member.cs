using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CashDesk.Model
{
    public class Member: IMember
    {

        public int MemberNumber { get; set; }

        public int MemberID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime Birthday { get; set; }

        public List<Membership> Memberships { get; set; }
    }

    public class Membership: IMembership
    {
        public int MemberShipID { get; set; }

        [Required]
        public Member Member { get; set; }

        [Required]
        public DateTime Begin { get; set; }

        public DateTime End
        {
            get
            {
                return End;
            }
            set
            {
                //check if the value is bigger than the begin
                if (value > Begin)
                {
                    End = value;
                }
            }
        }

        public List<Deposit> Deposits { get; set; }

        IMember IMembership.Member => Member;
    }

    public class Deposit : IDeposit
    {
        public int DepositID { get; set; }

        [Required]
        public Membership Membership { get; set; }

        [Required]
        [Range(0, Double.MaxValue)]
        public decimal Amount { get; set; }

        IMembership IDeposit.Membership => Membership;
    }

    public class DepositStatistics : IDepositStatistics
    {
        public IMember Member { get; set; }

        public int Year { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
