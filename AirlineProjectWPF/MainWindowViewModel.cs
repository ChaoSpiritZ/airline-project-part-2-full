using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AirlineProject;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace AirlineProjectWPF
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public DBGeneratorFacade dbGeneratorFacade = new DBGeneratorFacade();
        public AnonymousUserFacade anonymousFacade = new AnonymousUserFacade();

        public DelegateCommand AddToDB { get; set; }
        public DelegateCommand ReplaceDB { get; set; }

        public int Countries { get; set; }
        public int AirlineCompanies { get; set; }
        public int Customers { get; set; }
        public int FlightsPerCompany { get; set; }
        public int TicketsPerCustomer { get; set; }


        private string _logText;
        public string LogText
        {
            get
            {
                return _logText;
            }
            set
            {
                _logText = value;
                OnPropertyChanged("LogText");
            }
        }

        private string _progressText;
        public string ProgressText
        {
            get
            {
                return _progressText;
            }
            set
            {
                _progressText = value;
                OnPropertyChanged("ProgressText");
            }
        }

        private int _progressBar;
        public int ProgressBar
        {
            get
            {
                return _progressBar;
            }
            set
            {
                _progressBar = value;
                OnPropertyChanged("ProgressBar");
            }
        }

        public MainWindowViewModel()
        {
            AddToDB = new DelegateCommand(AddToDBExecute, AddToDBCanExecute);
            ReplaceDB = new DelegateCommand(ReplaceDBExecute, ReplaceDbCanExecute);

            ProgressText = "Unstarted";

            Task.Run(() =>
            {
                while (true)
                {
                    AddToDB.RaiseCanExecuteChanged();
                    ReplaceDB.RaiseCanExecuteChanged();
                    Thread.Sleep(10);
                }
            });

        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// adds rows to the database
        /// </summary>
        public void AddToDBExecute()
        {
            Task.Run(() =>
            {
                try
                {
                    CheckAddInputs(Countries, AirlineCompanies, Customers, FlightsPerCompany, TicketsPerCustomer);
                    ProgressBar = 0;
                    ProgressText = "Adding...";
                    LogText = string.Empty;
                    //Thread.Sleep(1000);
                    GetCountryInfo(Countries); //-------------------------------------------------------done
                    ProgressBar = 20;
                    LogText += string.Format($"- {Countries} Countries Added \n");
                    //Thread.Sleep(1000);
                    GetAirlineInfo(AirlineCompanies); //-----------------------------------------------done
                    ProgressBar = 40;
                    LogText += string.Format($"- {AirlineCompanies} Airline Companies Added \n");
                    //Thread.Sleep(1000);
                    GetCustomerInfo(Customers); //----------------------------------------------------done
                    ProgressBar = 60;
                    LogText += string.Format($"- {Customers} Customers Added \n");
                    //Thread.Sleep(1000);
                    GenerateFlights(FlightsPerCompany); //----------------------------------------------done
                    ProgressBar = 80;
                    LogText += string.Format($"- {FlightsPerCompany} Flights Per Company Added \n");
                    //Thread.Sleep(1000);
                    GenerateTickets(TicketsPerCustomer); //--------------------------------------------------------done i think
                    ProgressBar = 100;
                    LogText += string.Format($"- {TicketsPerCustomer} Tickets Per Customer Added \n");
                    ProgressText = "Completed!";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        public bool AddToDBCanExecute()
        {
            if (ProgressText == "Unstarted" || ProgressText == "Completed!")
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// clears database then adds rows to it
        /// </summary>
        public void ReplaceDBExecute()
        {
            Task.Run(() =>
            {
                try
                {
                    CheckReplaceInputs(Countries, AirlineCompanies, Customers, FlightsPerCompany, TicketsPerCustomer);
                    ProgressBar = 0;
                    ProgressText = "Replacing...";
                    LogText = string.Empty;
                    //Thread.Sleep(1000);
                    ClearDatabase();
                    ProgressBar = 16;
                    LogText += "- Cleared Database \n";
                    //Thread.Sleep(1000);
                    GetCountryInfo(Countries);
                    ProgressBar = 33;
                    LogText += string.Format($"- {Countries} Countries Added \n");
                    //Thread.Sleep(1000);
                    GetAirlineInfo(AirlineCompanies);
                    ProgressBar = 50;
                    LogText += string.Format($"- {AirlineCompanies} Airline Companies Added \n");
                    //Thread.Sleep(1000);
                    GetCustomerInfo(Customers);
                    ProgressBar = 66;
                    LogText += string.Format($"- {Customers} Customers Added \n");
                    //Thread.Sleep(1000);
                    GenerateFlights(FlightsPerCompany);
                    ProgressBar = 83;
                    LogText += string.Format($"- {FlightsPerCompany} Flights Per Company Added \n");
                    //Thread.Sleep(1000);
                    GenerateTickets(TicketsPerCustomer);
                    ProgressBar = 100;
                    LogText += string.Format($"- {TicketsPerCustomer} Tickets Per Customer Added \n");
                    ProgressText = "Completed!";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        public bool ReplaceDbCanExecute()
        {
            if (ProgressText == "Unstarted" || ProgressText == "Completed!")
            {
                return true;
            }
            return false;
        }

        public void GetCustomerInfo(int numOfCustomers)
        {
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri("https://randomuser.me/api");

            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            for (int i = 0; i < numOfCustomers; i++)
            {
                try
                {
                    // -------------------------- One item
                    HttpResponseMessage response = client.GetAsync("").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var dataObjects = response.Content.ReadAsStringAsync().Result;
                        JObject data = JObject.Parse(dataObjects);


                        var ja = JArray.Parse(data.GetValue("results").ToString());

                        // this is how to iterate over an array
                        //foreach(JObject child in ja.Children<JObject>())
                        //{
                        //    var jo = JObject.Parse(child.ToString());
                        //    var g1 = jo.GetValue("gender");
                        //}

                        var first = JObject.Parse(ja.Children<JObject>().First().ToString());

                        var firstName = JObject.Parse(first.GetValue("name").ToString()).GetValue("first").ToString();
                        var lastName = JObject.Parse(first.GetValue("name").ToString()).GetValue("last").ToString();
                        var userName = JObject.Parse(first.GetValue("login").ToString()).GetValue("username").ToString();
                        var password = JObject.Parse(first.GetValue("login").ToString()).GetValue("password").ToString();
                        var email = first.GetValue("email").ToString();
                        var phoneNumber = first.GetValue("cell").ToString();
                        var creditCardNumber = JObject.Parse(first.GetValue("login").ToString()).GetValue("uuid").ToString(); //there's no credit card number on the site so i took the uuid there instead

                        Customer customer = new Customer(0, firstName, lastName, userName, password, email, phoneNumber, creditCardNumber);

                        Console.WriteLine();
                        dbGeneratorFacade.CreateNewCustomer(customer);
                    }
                    else
                    {
                        Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                    }
                }
                catch (UsernameAlreadyExistsException ex)
                {
                    i--;
                }
            }
            client.Dispose();
        }

        public void GetCountryInfo(int numOfCountries)
        {
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri("https://restcountries.eu/rest/v2");

            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            int CellNumber = 0;
            for (int i = 0; i < numOfCountries; i++)
            {
                try
                {
                    // -------------------------- One item
                    HttpResponseMessage response = client.GetAsync("").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var dataObjects = response.Content.ReadAsStringAsync().Result;
                        var ja = JArray.Parse(dataObjects);

                        var first = JObject.Parse(ja.Children<JObject>().First().ToString());
                        var number = JObject.Parse(ja.Children<JObject>().ElementAt(CellNumber).ToString());

                        var name = number.GetValue("name").ToString();

                        Country country = new Country(0, name);
                        dbGeneratorFacade.CreateNewCountry(country);
                        CellNumber++;
                    }
                    else
                    {
                        Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                    }
                }
                catch (CountryAlreadyExistsException ex) //fixes the problem until i get to the site's country limit
                {
                    i--;
                    CellNumber++;
                }
            }
            client.Dispose();
        }

        public void GetAirlineInfo(int numOfAirlines)
        {
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri("https://randomuser.me/api");

            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            string[] texts = File.ReadAllLines("C:/Users/user/source/repos/AirlineProject-8-2-2020-home/AirlineProject/airlines.txt");
            int rowNumber = 0;

            Random rng = new Random();

            for (int i = 0; i < numOfAirlines; i++)
            {
                try
                {
                    //getting an airline name:
                    int startIndex = texts[rowNumber].IndexOf('"') + 1;
                    int endIndex = texts[rowNumber].IndexOf('"', startIndex);
                    string airlineName = texts[rowNumber].Substring(startIndex, (endIndex - startIndex));

                    //getting a random country from the database:
                    IList<Country> countries = dbGeneratorFacade.GetAllCountries();

                    long randomCountryCode = countries[rng.Next(0, countries.Count - 1)].ID;

                    // -------------------------- One item
                    HttpResponseMessage response = client.GetAsync("").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var dataObjects = response.Content.ReadAsStringAsync().Result;
                        JObject data = JObject.Parse(dataObjects);


                        var ja = JArray.Parse(data.GetValue("results").ToString());

                        var first = JObject.Parse(ja.Children<JObject>().First().ToString());

                        var userName = JObject.Parse(first.GetValue("login").ToString()).GetValue("username").ToString();
                        var password = JObject.Parse(first.GetValue("login").ToString()).GetValue("password").ToString();

                        AirlineCompany airline = new AirlineCompany(0, airlineName, userName, password, randomCountryCode);

                        Console.WriteLine();
                        dbGeneratorFacade.CreateNewAirline(airline);
                        rowNumber++;
                    }
                    else
                    {
                        Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                    }
                }
                catch (Exception ex) //should i generalize like this or should i catch the 3 exceptions individually? (they all get the same treatment nontheless)
                {
                    i--;
                    rowNumber++;
                }
            }
            client.Dispose();
        }

        public void GenerateFlights(int flightsPerCompany)
        {
            Random rng = new Random();
            DateTime now = DateTime.Now; //what's better? more fields but less text or less fields but more text?
            IList<AirlineCompany> airlines = anonymousFacade.GetAllAirlineCompanies();
            IList<Country> countries = dbGeneratorFacade.GetAllCountries();

            foreach (var airline in airlines)
            {
                for (int i = 0; i < flightsPerCompany; i++)
                {
                    try
                    {
                        //choosing random countries
                        int originCountryCodeIndex = rng.Next(0, countries.Count - 1);
                        int destinationCountryCodeIndex = rng.Next(0, countries.Count - 1);

                        Country originCountry = countries[originCountryCodeIndex];
                        Country destinationCountry = countries[destinationCountryCodeIndex];

                        //choosing random dates
                        int num1 = 0;
                        int num2 = 0;

                        while (num1 == num2)
                        {
                            num1 = rng.Next(-604800, 604801);
                            num2 = rng.Next(-604800, 604801);
                        }

                        DateTime departureDate;
                        DateTime landingDate;

                        if (num1 < num2)
                        {
                            departureDate = now.AddSeconds(num1);
                            landingDate = now.AddSeconds(num2);
                        }
                        else
                        {
                            departureDate = now.AddSeconds(num2);
                            landingDate = now.AddSeconds(num1);
                        }

                        Flight flight = new Flight(0, airline.ID, originCountry.ID, destinationCountry.ID, departureDate, landingDate, 200);
                        dbGeneratorFacade.CreateFlight(flight);
                    }
                    catch (Exception ex)
                    {
                        i--;
                    }
                }
            }
        }

        public void GenerateTickets(int ticketsPerCustomer)
        {
            //checking every scenario:
            /*
             * no problems at all - V
             * has tickets to all flights already - V
             * all flights are full - V
             * half-way through has tickets to all flights already - stopped thinking, but i think it's all good
             * half-way through all flights are full - 
             */

            Random rng = new Random();
            IList<Flight> flights = anonymousFacade.GetAllFlights();
            IList<Customer> customers = dbGeneratorFacade.GetAllCustomers();
            int noTicketsTries = 1000; //because every single customer might try to get a ticket to a full flight

            foreach (var customer in customers)
            {
                if (noTicketsTries <= 0)
                {
                    break;
                }

                int personalTries = 1000;

                for (int i = 0; i < ticketsPerCustomer; i++)
                {
                    if (noTicketsTries <= 0)
                    {
                        break;
                    }

                    if(personalTries <= 0)
                    {
                        break;
                    }

                    try
                    {
                        Flight randomFlight = flights[rng.Next(0, flights.Count - 1)];
                        dbGeneratorFacade.CreateTicket(customer, randomFlight); 
                        //what if no tickets available? (there's a tries system now)
                        //what if i have tickets to all flights? (there are personal tries now)
                    }
                    catch (TicketAlreadyExistsException ex)
                    {
                        i--;
                        personalTries--;
                    }
                    catch (NoMoreTicketsException ex)
                    {
                        i--;
                        noTicketsTries--;
                    }
                }
            }
        }

        public void CheckAddInputs(int numOfCountries, int numOfAirlines, int numOfCustomers, int flightsPerCompany, int ticketsPerCustomer)
        {
            //basic number check
            if (numOfCountries < 0 || numOfCountries > 200)
            {
                throw new InvalidInputsException("Action failed, number of countries should be between 0 and 200");
            }
            if (numOfAirlines < 0 || numOfAirlines > 200)
            {
                throw new InvalidInputsException("Action failed, number of airlines should be between 0 and 200");
            }
            if (numOfCustomers < 0 || numOfCustomers > 200)
            {
                throw new InvalidInputsException("Action failed, number of customers should be between 0 and 200");
            }
            if (flightsPerCompany < 0 || flightsPerCompany > 200)
            {
                throw new InvalidInputsException("Action failed, number of flights per company should be between 0 and 200");
            }
            if (ticketsPerCustomer < 0 || ticketsPerCustomer > 200)
            {
                throw new InvalidInputsException("Action failed, number of tickets per customer should be between 0 and 200");
            }

            //further country check
            IList<Country> countries = dbGeneratorFacade.GetAllCountries();

            if (countries.Count + numOfCountries > 200)
            {
                throw new InvalidInputsException($"Action failed, the max number of countries allowed is 200 (current number of countries: {countries.Count}/200)");
            }



            //further ticket check --------might or might need to move it to a different method

            //check if the number of seats available for the most crowded flight is higher than {the total number of customers}

            IList<Flight> flights = anonymousFacade.GetAllFlights();
            IList<Customer> customers = dbGeneratorFacade.GetAllCustomers();
            IList<AirlineCompany> airlines = anonymousFacade.GetAllAirlineCompanies();

            //checking if flights are vacant enough
            int leastVacantFlightVacancy = 200;

            foreach (var flight in flights)
            {
                if (flight.RemainingTickets < leastVacantFlightVacancy)
                {
                    leastVacantFlightVacancy = flight.RemainingTickets;
                }

                if (leastVacantFlightVacancy < customers.Count + numOfCustomers)
                {
                    //might not be necessary
                    //throw new InvalidInputsException($"Action failed, the most full flight has only [{leastVacantFlightVacancy}] seats available, which is less than the total number of customers after this action [{customers.Count + numOfCustomers}]");
                }
            }

            //checking customers' tickets
            if (ticketsPerCustomer > flights.Count + ((airlines.Count + numOfAirlines) * flightsPerCompany))
            {
                throw new InvalidInputsException($"Action failed, number of tickets per customer ({ticketsPerCustomer}) can't be higher than the total number of flights after this action ({flights.Count + ((airlines.Count + numOfAirlines) * flightsPerCompany)})");
            }

            int maximumTicketsPossible = 200;

            foreach (var customer in customers)
            {
                IList<Ticket> tickets = dbGeneratorFacade.GetAllTicketsOfCustomer(customer);
                if ((flights.Count + ((airlines.Count + numOfAirlines) * flightsPerCompany)) - tickets.Count < maximumTicketsPossible)
                {
                    maximumTicketsPossible = flights.Count - tickets.Count;
                }

                if (maximumTicketsPossible < ticketsPerCustomer)
                {
                    throw new InvalidInputsException($"Action failed, number of tickets per customer inputed is ({ticketsPerCustomer}), while the maximum number of tickets customers can currently have is ({maximumTicketsPossible})");
                }
            }
        }

        //no, it's not the same as CheckAddInputs()
        public void CheckReplaceInputs(int numOfCountries, int numOfAirlines, int numOfCustomers, int flightsPerCompany, int ticketsPerCustomer)
        {
            //basic number check
            if (numOfCountries < 0 || numOfCountries > 200)
            {
                throw new InvalidInputsException("Action failed, number of countries should be between 0 and 200");
            }
            if (numOfAirlines < 0 || numOfAirlines > 200)
            {
                throw new InvalidInputsException("Action failed, number of airlines should be between 0 and 200");
            }
            if (numOfCustomers < 0 || numOfCustomers > 200)
            {
                throw new InvalidInputsException("Action failed, number of customers should be between 0 and 200");
            }
            if (flightsPerCompany < 0 || flightsPerCompany > 200)
            {
                throw new InvalidInputsException("Action failed, number of flights per company should be between 0 and 200");
            }
            if (ticketsPerCustomer < 0 || ticketsPerCustomer > 200)
            {
                throw new InvalidInputsException("Action failed, number of tickets per customer should be between 0 and 200");
            }

            //checking customers' tickets
            if (ticketsPerCustomer > numOfAirlines * flightsPerCompany)
            {
                throw new InvalidInputsException($"Action failed, number of tickets per customer ({ticketsPerCustomer}) can't be higher than the total number of flights after this action ({numOfAirlines * flightsPerCompany})");
            }
        }

        public void ClearDatabase()
        {
            //in that order:
            //tickets
            //flights
            //customers
            //airlines
            //countries

            dbGeneratorFacade.ClearTables();
        }
    }
}
