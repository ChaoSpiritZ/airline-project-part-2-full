using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirlineProject
{
    public class DBGeneratorFacade : FacadeBase
    {
        public void CreateNewCustomer(Customer customer)
        {
            POCOValidator.CustomerValidator(customer, false);
            if (_airlineDAO.GetAirlineByUsername(customer.UserName) != null || _customerDAO.GetCustomerByUsername(customer.UserName) != null || customer.UserName == "admin")
                throw new UsernameAlreadyExistsException($"failed to create customer! Username '{customer.UserName}' is already taken!");
            _customerDAO.Add(customer);
        }

        public void CreateNewCountry(Country country)
        {
            POCOValidator.CountryValidator(country, false);
            if (_countryDAO.GetCountryByName(country.CountryName) != null)
                throw new CountryAlreadyExistsException($"failed to create country! Country name '{country.CountryName}' already exists!");
            _countryDAO.Add(country);
        }

        public void CreateNewAirline(AirlineCompany airline)
        {
            POCOValidator.AirlineCompanyValidator(airline, false);
            if (_airlineDAO.GetAirlineByAirlineName(airline.AirlineName) != null)
                throw new AirlineNameAlreadyExistsException($"failed to create airline! there is already an airline with the name '{airline.AirlineName}'");
            if (_airlineDAO.GetAirlineByUsername(airline.UserName) != null || _customerDAO.GetCustomerByUsername(airline.UserName) != null || airline.UserName == "admin")
                throw new UsernameAlreadyExistsException($"failed to create airline! Username '{airline.UserName}' is already taken!");
            if (_countryDAO.Get(airline.CountryCode) == null)
                throw new CountryNotFoundException($"failed to create airline! there is no country with id [{airline.CountryCode}]");
            _airlineDAO.Add(airline);
        }

        public void CreateFlight(Flight flight)
        {
            POCOValidator.FlightValidator(flight, false);
            if (DateTime.Compare(flight.DepartureTime, flight.LandingTime) > 0)
                throw new InvalidFlightDateException($"failed to create flight [{flight}], cannot fly back in time from [{flight.DepartureTime}] to [{flight.LandingTime}]");
            if (DateTime.Compare(flight.DepartureTime, flight.LandingTime) == 0)
                throw new InvalidFlightDateException($"failed to create flight [{flight}], departure time and landing time are the same [{flight.DepartureTime}], and as you know, teleportation isn't invented yet");
            if (_countryDAO.Get(flight.OriginCountryCode) == null)
                throw new CountryNotFoundException($"failed to create flight [{flight}], origin country with id [{flight.OriginCountryCode}] was not found!");
            if (_countryDAO.Get(flight.DestinationCountryCode) == null)
                throw new CountryNotFoundException($"failed to create flight [{flight}], destination country with id [{flight.DestinationCountryCode}] was not found!");
            _flightDAO.Add(flight);
        }

        public void CreateTicket(Customer customer, Flight flight)
        {
            POCOValidator.CustomerValidator(customer, false);
            POCOValidator.FlightValidator(flight, false);
            if (_flightDAO.Get(flight.ID) == null)
                throw new FlightNotFoundException($"failed to purchase ticket, there is no flight with id of [{flight.ID}]");
            IList<Ticket> tickets = _ticketDAO.GetTicketsByCustomerId(customer);
            if (tickets.Any(item => item.FlightId == flight.ID)) //boolean
                throw new TicketAlreadyExistsException($"failed to purchase ticket, you already purchased a ticket to flight [{flight}]"); //must be before checking if all seats are taken
            if (_flightDAO.Get(flight.ID).RemainingTickets == 0)
                throw new NoMoreTicketsException($"failed to purchase ticket to flight [{flight}], there are no more tickets left!");
            Ticket newTicket = new Ticket(0, flight.ID, customer.ID);
            _ticketDAO.Add(newTicket);
            flight.RemainingTickets--;
            _flightDAO.Update(flight);
        }

        public IList<Country> GetAllCountries()
        {
            IList<Country> countries = _countryDAO.GetAll();
            return countries;
        }

        public IList<Customer> GetAllCustomers()
        {
            IList<Customer> customers = _customerDAO.GetAll();
            return customers;
        }

        public IList<Ticket> GetAllTicketsOfCustomer(Customer customer)
        {
            IList<Ticket> tickets = _ticketDAO.GetTicketsByCustomerId(customer);
            return tickets;
        }

        public void ClearTables()
        {
            //in that order:
            //tickets
            //flights
            //customers
            //airlines
            //countries

            _ticketDAO.ClearTickets();
            _flightDAO.ClearFlights();
            _customerDAO.ClearCustomers();
            _airlineDAO.ClearAirlineCompanies();
            _countryDAO.ClearCountries();
        }
    }
}
