using CashDesk.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace CashDesk
{
    /// <inheritdoc />
    public class DataAccess : IDataAccess
    {
        DataAccessContext context;

        /// <inheritdoc />
        public Task InitializeDatabaseAsync()
        {
            if(context == null)
            {
                context = new DataAccessContext();
            }
            else
            {
                throw new InvalidOperationException("Database already initialized");
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task<int> AddMemberAsync(string firstName, string lastName, DateTime birthday)
        {
            if (context == null)
            {
                throw new InvalidOperationException("Database hasn't been initialised yet");
            }

            if (String.IsNullOrEmpty(firstName))
            {
                throw new ArgumentException("First-Name of Person cannot be empty");
            }

            if (String.IsNullOrEmpty(lastName))
            {
                throw new ArgumentException("Last-Name of Person cannot be empty");
            }

            if (await context.Members.AnyAsync(m => m.LastName == lastName))
            {
                throw new DuplicateNameException("Error: Duplicate name!");
            }

            Member member = new Member();
            member.FirstName = firstName;
            member.LastName = lastName;
            member.Birthday = birthday;

            context.Members.Add(member);
            context.SaveChanges();

            return member.MemberNumber;
        }

        /// <inheritdoc />
        public async Task DeleteMemberAsync(int memberNumber) {

            if(context == null)
            {
                throw new InvalidOperationException("Database hasn't been initialised yet");
            }

            if (!await context.Members.AnyAsync(m => m.MemberID == memberNumber))
            {
                throw new ArgumentException("No member with MemberID \"" + memberNumber + "\" found!");
            }

            Member member = await context.Members.FirstAsync(m => m.MemberID == memberNumber);
            context.Members.Remove(member);
            context.SaveChanges();
        }

        /// <inheritdoc />
        public async Task<IMembership> JoinMemberAsync(int memberNumber)
        {
            if (context == null)
            {
                throw new InvalidOperationException("Database hasn't been initialised yet");
            }

            if (!await context.Members.AnyAsync(m => m.MemberNumber == memberNumber))
            {
                throw new ArgumentException("Member with MemberID \"" + memberNumber + "\" is not a known Member!");
            }

            //Get member
            Member member = await context.Members.FirstAsync(m => m.MemberNumber == memberNumber);

            //Check if member has any memberships
            if (member.Memberships == null)
            {
                member.Memberships = new List<Membership>();
            }

            if (member.Memberships.Count == 0 || member.Memberships[member.Memberships.Count - 1].End != DateTime.MaxValue)
            {
                Membership membership = new Membership();
                membership.Member = member;
                membership.Begin = DateTime.Now;
                context.Memberships.Add(membership);
                context.SaveChanges();

                return membership;
            }
            else
            {
                throw new AlreadyMemberException("Member with MemberID \"" + memberNumber + "\" is already in a Membership");
            }

        }

        /// <inheritdoc />
        public async Task<IMembership> CancelMembershipAsync(int memberNumber)
        {
            if (context == null)
            {
                throw new InvalidOperationException("Database hasn't been initialised yet");
            }

            if (!await context.Members.AnyAsync(m => m.MemberNumber == memberNumber))
            {
                throw new ArgumentException("Member with MemberID \"" + memberNumber + "\" is not a known Member!");
            }

            //Get member
            Member member = await context.Members.FirstAsync(m => m.MemberNumber == memberNumber);

            if (member.Memberships != null && member.Memberships.Count > 0 && member.Memberships[member.Memberships.Count - 1].End == DateTime.MaxValue)
            {
                int membershipID = member.Memberships[member.Memberships.Count - 1].MemberShipID;

                Membership membership = await context.Memberships.FirstAsync(ms => ms.MemberShipID == membershipID);
                membership.End = DateTime.Now;

                context.Memberships.Update(membership);
                context.SaveChanges();

                return membership;
            }
            else
            {
                throw new NoMemberException("Member with MemberID \"" + memberNumber + "\" is not in a Membership!");
            }
        }

        /// <inheritdoc />
        public async Task DepositAsync(int memberNumber, decimal amount)
        {
            if (context == null)
            {
                throw new InvalidOperationException("Database hasn't been initialised yet");
            }

            //Check if member with memberId exists
            if (!await context.Members.AnyAsync(m => m.MemberNumber == memberNumber))
            {
                throw new ArgumentException("Member with MemberID \"" + memberNumber + "\" not found!");
            }

            if (amount < 0)
            {
                throw new ArgumentException("Amount \"" + amount + "\" was less than zero which is not allowed");
            }

            Member member = await context.Members.FirstAsync(m => m.MemberNumber == memberNumber);


            if (member.Memberships != null && member.Memberships.Count > 0 && member.Memberships[member.Memberships.Count - 1].End == DateTime.MaxValue)
            {
                //Get the last membership of member
                Membership membership = member.Memberships[member.Memberships.Count - 1];

                //Create new deposit
                Deposit deposit = new Deposit();

                //Set the deposit for membership and set amount of deposit
                deposit.Membership = membership;
                deposit.Amount = amount;

                context.Deposits.Add(deposit);
                context.SaveChanges();
            }
            else
            {
                throw new NoMemberException("Member with MemberID \"" + memberNumber + "\" isn't in a membership");
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDepositStatistics>> GetDepositStatisticsAsync()
        {
            if (context == null)
            {
                throw new InvalidOperationException("Database hasn't been initialised yet");
            }

            //Get lits of members and list of depositstatistics
            List<Member> members = await context.Members.ToListAsync();
            List<DepositStatistics> statistics = new List<DepositStatistics>();

            //iterate through members
            foreach (var member in members)
            {

                //Create new depositstatistic
                DepositStatistics stat = new DepositStatistics();
                //Set member
                stat.Member = member;

                if (member.Memberships.Count > 0 && member.Memberships != null && member.Memberships[member.Memberships.Count - 1].End == DateTime.MaxValue)
                {
                    Membership membershipOfMember = member.Memberships.ElementAt(member.Memberships.Count - 1);
                    decimal curTotalAmount = 0;
                    foreach (var curDeposit in membershipOfMember.Deposits)
                    {
                        curTotalAmount += curDeposit.Amount;
                    }
                    stat.TotalAmount = curTotalAmount;
                }
                else
                {
                    stat.TotalAmount = 0;
                }

                statistics.Add(stat);
            }

            return statistics;
        }

        /// <inheritdoc />
        public void Dispose() { }
    }
}
