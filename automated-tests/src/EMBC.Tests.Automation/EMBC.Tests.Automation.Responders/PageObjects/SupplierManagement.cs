﻿using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMBC.Tests.Automation.Responders.PageObjects
{

    public class SupplierManagement: PageObjectBase
    {

        private By suppliersMgmtTab = By.XPath("//mat-sidenav-content[1]/app-top-nav-menu[1]/mat-toolbar[1]/div[1]/a[4]");
        private By addSupplierTab = By.CssSelector("div[class='mat-tab-links'] a:nth-child(2)");
        private By addSupplierLegalNameInput = By.CssSelector("input[formcontrolname='supplierLegalName']");
        private By addSupplierNameInput = By.CssSelector("input[formcontrolname='supplierName']");
        private By addSupplierGstNbr1Input = By.CssSelector("input[formcontrolname='part1']");
        private By addSupplierGstNbr2Input = By.CssSelector("input[formcontrolname='part2']");
        private By addSupplierAddressLine1 = By.CssSelector("input[formcontrolname='addressLine1']");
        private By addSupplierAddressLine2 = By.CssSelector("input[formcontrolname='addressLine2']");
        private By addSupplierCitySelect = By.CssSelector("input[formcontrolname='community']");
        private By addSupplierPostalCodeInput = By.CssSelector("input[formcontrolname='postalCode']");
        private By addSupplierLastNameInput = By.CssSelector("input[formcontrolname='lastName']");
        private By addSupplierFirstNameInput = By.CssSelector("input[formcontrolname='firstName']");
        private By addSupplierPhoneInput = By.CssSelector("input[formcontrolname='phone']");
        private By addSupplierEmailInput = By.CssSelector("input[formcontrolname='email']");


        private By searchSupplierInput = By.Id("searchInput");
        private By suppliersManagementMainTableRows = By.CssSelector("table[role='table'] tbody tr");
        private By suppliersManagementMainTableFirstRows = By.CssSelector("table[role='table'] tbody tr:nth-child(1)");
        private By suppliersManagementMainTableFirstRowStatus = By.CssSelector("table[role='table'] tbody tr:nth-child(1) td:nth-child(6)");
        private By suppliersManagementStatusToggle = By.TagName("mat-slide-toggle");

        private By deleteTeamMemberCheckBox = By.Id("confirmDelete");

        public SupplierManagement(IWebDriver webDriver) : base(webDriver)
        { }

        public void EnterSupplierManagement()
        {
            webDriver.FindElement(suppliersMgmtTab).Click();
        }

        public void AddNewSupplierTab()
        {
            webDriver.FindElement(addSupplierTab).Click();
        }

        public void AddNewSupplier(string legalName, string name, string gstPart1, string gstPart2, string addressLine1, string addressLine2, string city, string zipcode, string lastName, string firstName, string phone, string email)
        {
            Wait();
            webDriver.FindElement(addSupplierLegalNameInput).SendKeys(legalName);
            webDriver.FindElement(addSupplierNameInput).SendKeys(name);
            webDriver.FindElement(addSupplierGstNbr1Input).SendKeys(gstPart1);
            webDriver.FindElement(addSupplierGstNbr2Input).SendKeys(gstPart2);
            ButtonElement("Next");

            Wait();
            webDriver.FindElement(addSupplierAddressLine1).SendKeys(addressLine1);
            webDriver.FindElement(addSupplierAddressLine2).SendKeys(addressLine2);
            webDriver.FindElement(addSupplierCitySelect).SendKeys(city);
            webDriver.FindElement(By.Id(city)).Click();
            webDriver.FindElement(addSupplierPostalCodeInput).SendKeys(zipcode);

            webDriver.FindElement(addSupplierLastNameInput).SendKeys(lastName);
            webDriver.FindElement(addSupplierFirstNameInput).SendKeys(firstName);
            webDriver.FindElement(addSupplierPhoneInput).SendKeys(phone);
            webDriver.FindElement(addSupplierEmailInput).SendKeys(email);
            ButtonElement("Next");

            Wait();
            ButtonElement("Save Supplier");

            Wait();
            ButtonElement("Close");

        }

        public void SearchSupplier(string searchInput)
        {
            webDriver.FindElement(searchSupplierInput).Clear();
            webDriver.FindElement(searchSupplierInput).SendKeys(searchInput);
            ButtonElement("Search");

        }

        public void ChangeSupplierStatus()
        {
            Wait();
            webDriver.FindElement(suppliersManagementStatusToggle).Click();
        }

        public void SelectSupplier()
        {
            Wait();
            webDriver.FindElement(suppliersManagementMainTableFirstRows).Click();
        }

        public void DeleteSupplier()
        {
            Wait();
            ButtonElement("Remove Supplier");

            Wait();
            ButtonElement("Yes, Remove Supplier");

            Wait();
            ButtonElement("Close");
        }

        // ASSERT FUNCTIONS

        public string GetMemberStatus()
        {
            Wait();
            return webDriver.FindElement(suppliersManagementMainTableFirstRowStatus).Text;
        }

        public int GetTotalSearchNumber()
        {
            Wait();
            return webDriver.FindElements(suppliersManagementMainTableRows).Count;

        }
    }
}
