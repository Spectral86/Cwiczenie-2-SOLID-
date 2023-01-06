using System;
using System.Runtime.CompilerServices;

namespace LegacyApp
{
    public class UserService
    {
        //AddUser_ShouldAddUserCorrectly
        //AddUser_ShouldFail_IncorrectEmail

        //SOLID
        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            //walidacja emaila start
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                return false;
            }

            if (!email.Contains("@") && !email.Contains("."))
            {
                return false;
            }
            // walidacja emaila koniec    

            //zmienna DateTime typu integer deklarująca datę urodzenia rok, miesiąc dzień
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month<dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day<dateOfBirth.Day)) age--;
            //Nie dopuszcza klientów poniżej 21 lat
            if (age< 21)
            {
                return false;
            }
    //ta cała zmienna mogłaby być wrzucona do klasy ClientRepository
            //zmienna ClientReposytory deklarująca dane klienta: datę urodzenia, adres email, imię i nazwisko na podstawie ID klienta
            var clientRepository = new ClientRepository();
            var client = clientRepository.GetById(clientId);

            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };
            //jeśli klient jest VIPem to brak limitu kredytowego
            if (client.Name == "VeryImportantClient")
            {
                //Skip credit limit
                user.HasCreditLimit = false;
            }//Jeśli klient jest ważny to użyj klasy UserCreditService, żeby określić limit kredytu do 10000 i pomnóż limit *2
            else if (client.Name == "ImportantClient")
            {
                using (var userCreditService = new UserCreditService())
                {
                    int creditLimit = userCreditService.GetCreditLimit(user.FirstName, user.LastName, user.DateOfBirth);
                    creditLimit = creditLimit * 2;
                    user.CreditLimit = creditLimit;
                }
            }
            else
            {
                //Do credit check
                user.HasCreditLimit = true;
                using (var userCreditService = new UserCreditService())
                {
                    int creditLimit = userCreditService.GetCreditLimit(user.FirstName, user.LastName, user.DateOfBirth);
                    user.CreditLimit = creditLimit;
                }
            }
            //jeśli klient już ma przyznany limit kredytowy lub jego limit wynosi poniżej < 500 zgłoś błąd
            if (user.HasCreditLimit && user.CreditLimit < 500)
            {
                return false;
            }
            // w przypadku gdy wszystko poprawne dodaj klienta
            UserDataAccess.AddUser(user);
            return true;
        }
    }
}
