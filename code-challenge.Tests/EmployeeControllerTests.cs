using challenge.Controllers;
using challenge.Data;
using challenge.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using code_challenge.Tests.Integration.Extensions;

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using code_challenge.Tests.Integration.Helpers;
using System.Text;
using challenge.API_Models;

namespace code_challenge.Tests.Integration
{
    [TestClass]
    public class EmployeeControllerTests
    {
        private static HttpClient _httpClient;
        private static TestServer _testServer;

        [ClassInitialize]
        public static void InitializeClass(TestContext context)
        {
            _testServer = new TestServer(WebHost.CreateDefaultBuilder()
                .UseStartup<TestServerStartup>()
                .UseEnvironment("Development"));

            _httpClient = _testServer.CreateClient();
        }

        [ClassCleanup]
        public static void CleanUpTest()
        {
            _httpClient.Dispose();
            _testServer.Dispose();
        }

        [TestMethod]
        public void CreateEmployee_Returns_Created()
        {
            // Arrange
            var employee = new Employee()
            {
                Department = "Complaints",
                FirstName = "Debbie",
                LastName = "Downer",
                Position = "Receiver",
            };

            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var postRequestTask = _httpClient.PostAsync("api/employee",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var newEmployee = response.DeserializeContent<Employee>();
            Assert.IsNotNull(newEmployee.EmployeeId);
            Assert.AreEqual(employee.FirstName, newEmployee.FirstName);
            Assert.AreEqual(employee.LastName, newEmployee.LastName);
            Assert.AreEqual(employee.Department, newEmployee.Department);
            Assert.AreEqual(employee.Position, newEmployee.Position);
        }

        [TestMethod]
        public void GetEmployeeById_Returns_Ok()
        {
            // Arrange
            var employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";
            var expectedFirstName = "John";
            var expectedLastName = "Lennon";

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var employee = response.DeserializeContent<Employee>();
            Assert.AreEqual(expectedFirstName, employee.FirstName);
            Assert.AreEqual(2, employee.DirectReports.Count);

        }

        //base test to make sure we cover employees without reports

        [TestMethod]
        public void GetReportingStructureShould_Return_Employee_and_No_Reports()
        {
            var employeeId = "c0c2293d-16bd-4603-8e08-638a9d18b22c";

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}/reportingStructure");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var reportingStructure = response.DeserializeContent<ReportingStructure>();
            Assert.AreEqual(0, reportingStructure?.numberOfReports);
            Assert.AreEqual(employeeId, reportingStructure?.employee?.EmployeeId);


        }
        // tests bases case where no employee id is provided
        [TestMethod]
        public void GetReportingStructureShould_Return_null()
        {
            string employeeId = null;
            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}/reportingStructure");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            var reportingStructure = response.DeserializeContent<ReportingStructure>();
            Assert.IsNull(reportingStructure);
        }
        // tests can't find emplloyee case where no employee id is provided
        [TestMethod]
        public void GetReportingStructureShould_Return_null_ID_ID_Not_Found()
        {
            string employeeId = "AAAA";
            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}/reportingStructure");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            var reportingStructure = response.DeserializeContent<ReportingStructure>();
            Assert.IsNull(reportingStructure);
        }
        // tests for returning number of reports for a given id
        [TestMethod]
        public void GetReportingStructureShould_Return_Number_OfReports_ForGrandChildren()
        {
            // for this test we want the context to be in a known good state. For this we are recreating the context
            string employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";
            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}/reportingStructure");
            var response = getRequestTask.Result;

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var reportingStructure = response.DeserializeContent<ReportingStructure>();
            Assert.AreEqual(4, reportingStructure?.numberOfReports);
            Assert.AreEqual(employeeId, reportingStructure?.employee?.EmployeeId);
        }

        [TestMethod]
        public void UpdateEmployee_Returns_Ok()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "03aa1462-ffa9-4978-901b-7c001562cf6f",
                Department = "Engineering",
                FirstName = "Pete",
                LastName = "Best",
                Position = "Developer VI",
            };
            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var putRequestTask = _httpClient.PutAsync($"api/employee/{employee.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var putResponse = putRequestTask.Result;
            
            // Assert
            Assert.AreEqual(HttpStatusCode.OK, putResponse.StatusCode);
            var newEmployee = putResponse.DeserializeContent<Employee>();

            Assert.AreEqual(employee.FirstName, newEmployee.FirstName);
            Assert.AreEqual(employee.LastName, newEmployee.LastName);
        }

        [TestMethod]
        public void UpdateEmployee_Returns_NotFound()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "Invalid_Id",
                Department = "Music",
                FirstName = "Sunny",
                LastName = "Bono",
                Position = "Singer/Song Writer",
            };
            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var postRequestTask = _httpClient.PutAsync($"api/employee/{employee.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }


        // //Salary and effective date are required
        [TestMethod]
        public void CreateCompensation_Required_Fields()
        {
            string employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";
            var requestContent =   new JsonSerialization().ToJson(new CreateCompensationRequest { });
            // Execute
            var getRequestTask = _httpClient.PostAsync($"api/employee/{employeeId}/compensation", new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = getRequestTask.Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
          
        }
        // //basic create of compensation
        [TestMethod]
        public void CreateCompensation()
        {
            string employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";
            var requestContent = new JsonSerialization().ToJson(new CreateCompensationRequest { Salary=500, EffectiveDate=new DateTime(2021,01,01)});
            // Execute
            var postRequestTask = _httpClient.PostAsync($"api/employee/{employeeId}/compensation", new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var result = response.DeserializeContent<CompensationResponse>();
            Assert.AreEqual(500, result.Salary);
            Assert.AreEqual(new DateTime(2021, 01, 01), result.EffectiveDate);
            Assert.AreEqual("16a596ae-edd3-4847-99fe-c4518e82c86f", result.Employee.EmployeeId);

        }
        // create and then get the compensation we created to prove it was peristed
        [TestMethod]
        public void GetCompensation_()
        {
            //create
            string employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";
            var requestContent = new JsonSerialization().ToJson(new CreateCompensationRequest { Salary = 500, EffectiveDate = new DateTime(2021, 01, 01) });
            // Execute
            var postRequestTask = _httpClient.PostAsync($"api/employee/{employeeId}/compensation", new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}/compensation");
             response = postRequestTask.Result;
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var result = response.DeserializeContent<CompensationResponse>();

            Assert.AreEqual(500, result.Salary);
            Assert.AreEqual(new DateTime(2021, 01, 01), result.EffectiveDate);
            Assert.AreEqual("16a596ae-edd3-4847-99fe-c4518e82c86f", result.Employee.EmployeeId);

        }

    }
}
