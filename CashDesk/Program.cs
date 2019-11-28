using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CashDesk.Model;

namespace CashDesk
{
    class Program
    {

        public static int main(String[] args)
        {
            return 0;
        }

        async static Task WriteToDB(DataAccessContext db)
        {

            db.Members.AddRange(new []
            {
                new Member() { MemberID=0, FirstName="Markus", LastName="Kurzmann", Birthday=DateTime.Parse("2001-03-14")},
                new Member() { MemberID=0, FirstName="Tim", LastName="Klecka", Birthday=DateTime.Parse("2000-10-01")},
                new Member() { MemberID=0, FirstName="Thomas", LastName="Brych", Birthday=DateTime.Parse("2000-10-23")},
                new Member() { MemberID=0, FirstName="Markus", LastName="Kurzmann", Birthday=DateTime.Parse("2000-11-09")}
            });

            await db.SaveChangesAsync();
        }
    }
}
