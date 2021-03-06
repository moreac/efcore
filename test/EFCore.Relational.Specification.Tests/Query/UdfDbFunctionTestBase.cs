﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class UdfDbFunctionTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : SharedStoreFixtureBase<DbContext>, new()
    {
        protected UdfDbFunctionTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        protected UDFSqlContext CreateContext() => (UDFSqlContext)Fixture.CreateContext();

        #region Model

        public enum PhoneType
        {
            Home = 0,
            Work = 1,
            Cell = 2
        }

        public enum CreditCardType
        {
            ShutupAndTakeMyMoney = 0,
            BuyNLarge = 1,
            BankOfDad = 2
        }

        public class CreditCard
        {
            public CreditCardType CreditCardType { get; set; }
            public string Number { get; set; }
        }

        public class PhoneInformation
        {
            public PhoneType PhoneType { get; set; }
            public string Number { get; set; }
        }

        public class Customer
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }

            public CreditCard CreditCard { get; set; }

            public List<Order> Orders { get; set; }
            public List<Address> Addresses { get; set; }
            public List<PhoneInformation> PhoneNumbers { get; set; }
        }

        public class Order
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime OrderDate { get; set; }

            public int CustomerId { get; set; }

            public Customer Customer { get; set; }
            public List<LineItem> Items { get; set; }
        }

        public class LineItem
        {
            public int Id { get; set; }
            public int OrderId { get; set; }
            public int ProductId { get; set; }
            public int Quantity { get; set; }

            public Order Order { get; set; }
            public Product Product { get; set; }
        }

        public class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class Address
        {
            public int Id { get; set; }
            public string Street { get; set; }
            public string City { get; set; }
            public string State { get; set; }

            public int CustomerId { get; set; }
            public Customer Customer { get; set; }
        }

        protected class UDFSqlContext : PoolableDbContext
        {
            #region DbSets

            public DbSet<Customer> Customers { get; set; }
            public DbSet<Order> Orders { get; set; }
            public DbSet<Product> Products { get; set; }
            public DbSet<Address> Addresses { get; set; }

            #endregion

            #region Function Stubs

            public enum ReportingPeriod
            {
                Winter = 0,
                Spring,
                Summer,
                Fall
            }

            public static long MyCustomLengthStatic(string s) => throw new Exception();
            public static bool IsDateStatic(string date) => throw new Exception();
            public static int AddOneStatic(int num) => num + 1;
            public static int AddFiveStatic(int number) => number + 5;
            public static int CustomerOrderCountStatic(int customerId) => throw new NotImplementedException();

            public static int CustomerOrderCountWithClientStatic(int customerId) => customerId switch
            {
                1 => 3,
                2 => 2,
                3 => 1,
                4 => 0,
                _ => throw new Exception()
            };

            public static string StarValueStatic(int starCount, int value) => throw new NotImplementedException();
            public static bool IsTopCustomerStatic(int customerId) => throw new NotImplementedException();
            public static int GetCustomerWithMostOrdersAfterDateStatic(DateTime? startDate) => throw new NotImplementedException();
            public static DateTime? GetReportingPeriodStartDateStatic(ReportingPeriod periodId) => throw new NotImplementedException();
            public static string GetSqlFragmentStatic() => throw new NotImplementedException();

            public long MyCustomLengthInstance(string s) => throw new Exception();
            public bool IsDateInstance(string date) => throw new Exception();
            public int AddOneInstance(int num) => num + 1;
            public int AddFiveInstance(int number) => number + 5;
            public int CustomerOrderCountInstance(int customerId) => throw new NotImplementedException();

            public int CustomerOrderCountWithClientInstance(int customerId) => customerId switch
            {
                1 => 3,
                2 => 2,
                3 => 1,
                4 => 0,
                _ => throw new Exception()
            };

            public string StarValueInstance(int starCount, int value) => throw new NotImplementedException();
            public bool IsTopCustomerInstance(int customerId) => throw new NotImplementedException();
            public int GetCustomerWithMostOrdersAfterDateInstance(DateTime? startDate) => throw new NotImplementedException();
            public DateTime? GetReportingPeriodStartDateInstance(ReportingPeriod periodId) => throw new NotImplementedException();
            public string DollarValueInstance(int starCount, string value) => throw new NotImplementedException();

            [DbFunction(Schema = "dbo")]
            public static string IdentityString(string s) => throw new Exception();

            public int AddValues(int a, int b)
            {
                throw new NotImplementedException();
            }

            public int AddValues(Expression<Func<int>> a, int b)
            {
                throw new NotImplementedException();
            }

            #region Queryable Functions

            public class OrderByYear
            {
                public int? CustomerId { get; set; }
                public int? Count { get; set; }
                public int? Year { get; set; }
            }

            public class MultProductOrders
            {
                public int OrderId { get; set; }

                public Customer Customer { get; set; }
                public int CustomerId { get; set; }

                public DateTime OrderDate { get; set; }
            }

            public IQueryable<OrderByYear> GetCustomerOrderCountByYear(int customerId)
            {
                return CreateQuery(() => GetCustomerOrderCountByYear(customerId));
            }

            public class TopSellingProduct
            {
                public Product Product { get; set; }
                public int? ProductId { get; set; }

                public int? AmountSold { get; set; }
            }

            public IQueryable<TopSellingProduct> GetTopTwoSellingProducts()
            {
                return CreateQuery(() => GetTopTwoSellingProducts());
            }

            public IQueryable<TopSellingProduct> GetTopSellingProductsForCustomer(int customerId)
            {
                return CreateQuery(() => GetTopSellingProductsForCustomer(customerId));
            }

            public IQueryable<MultProductOrders> GetOrdersWithMultipleProducts(int customerId)
            {
                return CreateQuery(() => GetOrdersWithMultipleProducts(customerId));
            }

            public IQueryable<CreditCard> GetCreditCards(int customerId)
            {
                return CreateQuery(() => GetCreditCards(customerId));
            }

            public IQueryable<PhoneInformation> GetPhoneInformation(int customerId, string areaCode)
            {
                return CreateQuery(() => GetPhoneInformation(customerId, areaCode));
            }

            #endregion

            #endregion

            public UDFSqlContext(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>().OwnsOne(typeof(CreditCard), "CreditCard");
                modelBuilder.Entity<Customer>().OwnsMany(typeof(PhoneInformation), "PhoneNumbers");

                //Static
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(CustomerOrderCountStatic))).HasName("CustomerOrderCount");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(CustomerOrderCountWithClientStatic)))
                    .HasName("CustomerOrderCount");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(StarValueStatic))).HasName("StarValue");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(IsTopCustomerStatic))).HasName("IsTopCustomer");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetCustomerWithMostOrdersAfterDateStatic)))
                    .HasName("GetCustomerWithMostOrdersAfterDate");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetReportingPeriodStartDateStatic)))
                    .HasName("GetReportingPeriodStartDate");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetSqlFragmentStatic)))
                    .HasTranslation(args => new SqlFragmentExpression("'Two'"));
                var isDateMethodInfo = typeof(UDFSqlContext).GetMethod(nameof(IsDateStatic));
                modelBuilder.HasDbFunction(isDateMethodInfo)
                    .HasTranslation(args => SqlFunctionExpression.Create(
                        "IsDate",
                        args,
                        nullable: true,
                        argumentsPropagateNullability: args.Select(a => true).ToList(),
                        isDateMethodInfo.ReturnType,
                        null));

                var methodInfo = typeof(UDFSqlContext).GetMethod(nameof(MyCustomLengthStatic));

                modelBuilder.HasDbFunction(methodInfo)
                    .HasTranslation(args => SqlFunctionExpression.Create(
                        "len",
                        args,
                        nullable: true,
                        argumentsPropagateNullability: args.Select(a => true).ToList(),
                        methodInfo.ReturnType,
                        null));

                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(AddValues), new[] { typeof(int), typeof(int) }));

                //Instance
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(CustomerOrderCountInstance)))
                    .HasName("CustomerOrderCount");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(CustomerOrderCountWithClientInstance)))
                    .HasName("CustomerOrderCount");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(StarValueInstance))).HasName("StarValue");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(IsTopCustomerInstance))).HasName("IsTopCustomer");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetCustomerWithMostOrdersAfterDateInstance)))
                    .HasName("GetCustomerWithMostOrdersAfterDate");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetReportingPeriodStartDateInstance)))
                    .HasName("GetReportingPeriodStartDate");
                var isDateMethodInfo2 = typeof(UDFSqlContext).GetMethod(nameof(IsDateInstance));
                modelBuilder.HasDbFunction(isDateMethodInfo2)
                    .HasTranslation(args => SqlFunctionExpression.Create(
                        "IsDate",
                        args,
                        nullable: true,
                        argumentsPropagateNullability: args.Select(a => true).ToList(),
                        isDateMethodInfo2.ReturnType,
                        null));

                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(DollarValueInstance))).HasName("DollarValue");

                var methodInfo2 = typeof(UDFSqlContext).GetMethod(nameof(MyCustomLengthInstance));

                modelBuilder.HasDbFunction(methodInfo2)
                    .HasTranslation(args => SqlFunctionExpression.Create(
                        "len",
                        args,
                        nullable: true,
                        argumentsPropagateNullability: args.Select(a => true).ToList(),
                        methodInfo2.ReturnType,
                        null));

                modelBuilder.Entity<MultProductOrders>().ToQueryableFunctionResultType().HasKey(mpo => mpo.OrderId);

                modelBuilder.Entity<OrderByYear>().ToQueryableFunctionResultType().HasNoKey();
                modelBuilder.Entity<TopSellingProduct>().ToQueryableFunctionResultType().HasNoKey();

                //Table
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetCustomerOrderCountByYear), new[] { typeof(int) }));
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetTopTwoSellingProducts)));
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetTopSellingProductsForCustomer)));
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetCreditCards)));
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetPhoneInformation)));

                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetOrdersWithMultipleProducts)));
            }
        }

        public abstract class UdfFixtureBase : SharedStoreFixtureBase<DbContext>
        {
            protected override Type ContextType { get; } = typeof(UDFSqlContext);

            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

            protected override bool ShouldLogCategory(string logCategory)
                => logCategory == DbLoggerCategory.Query.Name;

            protected override void Seed(DbContext context)
            {
                context.Database.EnsureCreatedResiliently();

                var product1 = new Product { Name = "Product1" };
                var product2 = new Product { Name = "Product2" };
                var product3 = new Product { Name = "Product3" };
                var product4 = new Product { Name = "Product4" };
                var product5 = new Product { Name = "Product5" };

                var order11 = new Order
                {
                    Name = "Order11", OrderDate = new DateTime(2000, 1, 20),
                    Items = new List<LineItem>
                    {
                        new LineItem { Quantity = 5, Product = product1},
                        new LineItem { Quantity = 15, Product = product3}
                    }
                };

                var order12 = new Order { Name = "Order12", OrderDate = new DateTime(2000, 2, 21),
                    Items = new List<LineItem>
                    {
                        new LineItem { Quantity = 1, Product = product1},
                        new LineItem { Quantity = 6, Product = product2},
                        new LineItem { Quantity = 200, Product = product3}
                    }
                };

                var order13 = new Order { Name = "Order13", OrderDate = new DateTime(2001, 3, 20),
                    Items = new List<LineItem>
                    {
                        new LineItem { Quantity = 50, Product = product4},
                    }
                };

                var order21 = new Order { Name = "Order21", OrderDate = new DateTime(2000, 4, 21),
                    Items = new List<LineItem>
                    {
                        new LineItem { Quantity = 1, Product = product1},
                        new LineItem { Quantity = 34, Product = product4},
                        new LineItem { Quantity = 100, Product = product5}
                    }
                };

                var order22 = new Order { Name = "Order22", OrderDate = new DateTime(2000, 5, 20),
                    Items = new List<LineItem>
                    {
                        new LineItem { Quantity = 34, Product = product3},
                        new LineItem { Quantity = 100, Product = product4}
                    }
                };

                var order31 = new Order { Name = "Order31", OrderDate = new DateTime(2001, 6, 21),
                    Items = new List<LineItem>
                    {
                        new LineItem { Quantity = 5, Product = product5}
                    }
                };

                var address11 = new Address { Street = "1600 Pennsylvania Avenue", City = "Washington", State = "DC" };
                var address12 = new Address { Street = "742 Evergreen Terrace", City = "SpringField", State = "" };
                var address21 = new Address { Street = "Apartment 5A, 129 West 81st Street", City = "New York", State = "NY" };
                var address31 = new Address { Street = "425 Grove Street, Apt 20", City = "New York", State = "NY" };
                var address32 = new Address { Street = "342 GravelPit Terrace", City = "BedRock", State = "" };
                var address41 = new Address { Street = "4222 Clinton Way", City = "Los Angles", State = "CA" };
                var address42 = new Address { Street = "1060 West Addison Street", City = "Chicago", State = "IL" };
                var address43 = new Address { Street = "112 ½ Beacon Street", City = "Boston", State = "MA" };

                var customer1 = new Customer
                {
                    FirstName = "Customer",
                    LastName = "One",
                    Orders = new List<Order> { order11, order12, order13 },
                    Addresses = new List<Address> { address11, address12 },
                    CreditCard = new CreditCard() {  CreditCardType = CreditCardType.BankOfDad, Number = "123"},
                    PhoneNumbers = new List<PhoneInformation>
                    {
                        new PhoneInformation { Number = "123-978-2342", PhoneType = PhoneType.Cell},
                        new PhoneInformation { Number = "654-323-2342", PhoneType = PhoneType.Home}
                    }
                };

                var customer2 = new Customer
                {
                    FirstName = "Customer",
                    LastName = "Two",
                    Orders = new List<Order> { order21, order22 },
                    Addresses = new List<Address> { address21 },
                    PhoneNumbers = new List<PhoneInformation>
                    {
                        new PhoneInformation { Number = "234-873-4921", PhoneType = PhoneType.Cell},
                        new PhoneInformation { Number = "345-345-9234", PhoneType = PhoneType.Home},
                        new PhoneInformation { Number = "923-913-1232", PhoneType = PhoneType.Work}
                    }
                };

                var customer3 = new Customer
                {
                    FirstName = "Customer",
                    LastName = "Three",
                    Orders = new List<Order> { order31 },
                    Addresses = new List<Address> { address31, address32 },
                    CreditCard = new CreditCard() { CreditCardType = CreditCardType.BuyNLarge, Number = "554355" },
                     PhoneNumbers = new List<PhoneInformation>
                    {
                        new PhoneInformation { Number = "789-834-0934", PhoneType = PhoneType.Cell},
                        new PhoneInformation { Number = "902-092-2342", PhoneType = PhoneType.Home},
                        new PhoneInformation { Number = "234-789-2345", PhoneType = PhoneType.Work}

                    }
                };

                var customer4 = new Customer
                {
                    FirstName = "Customer",
                    LastName = "Four",
                    Addresses = new List<Address> { address41, address42, address43 },
                    CreditCard = new CreditCard() { CreditCardType = CreditCardType.ShutupAndTakeMyMoney, Number = "99-99" },
                    PhoneNumbers = new List<PhoneInformation>
                    {
                        new PhoneInformation { Number = "269-980-9238", PhoneType = PhoneType.Work}
                    }
                };

                ((UDFSqlContext)context).Products.AddRange(product1, product2, product3, product4, product5);
                ((UDFSqlContext)context).Addresses.AddRange(address11, address12, address21, address31, address32, address41, address42, address43);
                ((UDFSqlContext)context).Customers.AddRange(customer1, customer2, customer3, customer4);
                ((UDFSqlContext)context).Orders.AddRange(order11, order12, order13, order21, order22, order31);
            }
        }

        #endregion

        #region Scalar Tests

        #region Static

        [ConditionalFact]
        public virtual void Scalar_Function_Extension_Method_Static()
        {
            using var context = CreateContext();

            var len = context.Customers.Count(c => UDFSqlContext.IsDateStatic(c.FirstName) == false);

            Assert.Equal(4, len);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_With_Translator_Translates_Static()
        {
            using var context = CreateContext();
            var customerId = 3;

            var len = context.Customers.Where(c => c.Id == customerId)
                .Select(c => UDFSqlContext.MyCustomLengthStatic(c.LastName)).Single();

            Assert.Equal(5, len);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_ClientEval_Method_As_Translateable_Method_Parameter_Static()
        {
            using var context = CreateContext();

            Assert.Throws<NotImplementedException>(
                () => (from c in context.Customers
                       where c.Id == 1
                       select new
                       {
                           c.FirstName, OrderCount = UDFSqlContext.CustomerOrderCountStatic(UDFSqlContext.AddFiveStatic(c.Id - 5))
                       }).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Constant_Parameter_Static()
        {
            using var context = CreateContext();
            var customerId = 1;

            var custs = context.Customers.Select(c => UDFSqlContext.CustomerOrderCountStatic(customerId)).ToList();

            Assert.Equal(4, custs.Count);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Anonymous_Type_Select_Correlated_Static()
        {
            using var context = CreateContext();

            var cust = (from c in context.Customers
                        where c.Id == 1
                        select new { c.LastName, OrderCount = UDFSqlContext.CustomerOrderCountStatic(c.Id) }).Single();

            Assert.Equal("One", cust.LastName);
            Assert.Equal(3, cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Anonymous_Type_Select_Not_Correlated_Static()
        {
            using var context = CreateContext();

            var cust = (from c in context.Customers
                        where c.Id == 1
                        select new { c.LastName, OrderCount = UDFSqlContext.CustomerOrderCountStatic(1) }).Single();

            Assert.Equal("One", cust.LastName);
            Assert.Equal(3, cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Anonymous_Type_Select_Parameter_Static()
        {
            using var context = CreateContext();
            var customerId = 1;

            var cust = (from c in context.Customers
                        where c.Id == customerId
                        select new { c.LastName, OrderCount = UDFSqlContext.CustomerOrderCountStatic(customerId) }).Single();

            Assert.Equal("One", cust.LastName);
            Assert.Equal(3, cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Anonymous_Type_Select_Nested_Static()
        {
            using var context = CreateContext();
            var customerId = 3;
            var starCount = 3;

            var cust = (from c in context.Customers
                        where c.Id == customerId
                        select new
                        {
                            c.LastName,
                            OrderCount = UDFSqlContext.StarValueStatic(
                                starCount, UDFSqlContext.CustomerOrderCountStatic(customerId))
                        }).Single();

            Assert.Equal("Three", cust.LastName);
            Assert.Equal("***1", cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Where_Correlated_Static()
        {
            using var context = CreateContext();

            var cust = (from c in context.Customers
                        where UDFSqlContext.IsTopCustomerStatic(c.Id)
                        select c.Id.ToString().ToLower()).ToList();

            Assert.Single(cust);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Where_Not_Correlated_Static()
        {
            using var context = CreateContext();
            var startDate = new DateTime(2000, 4, 1);

            var custId = (from c in context.Customers
                          where UDFSqlContext.GetCustomerWithMostOrdersAfterDateStatic(startDate) == c.Id
                          select c.Id).SingleOrDefault();

            Assert.Equal(2, custId);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Where_Parameter_Static()
        {
            using var context = CreateContext();
            var period = UDFSqlContext.ReportingPeriod.Winter;

            var custId = (from c in context.Customers
                          where c.Id
                              == UDFSqlContext.GetCustomerWithMostOrdersAfterDateStatic(
                                  UDFSqlContext.GetReportingPeriodStartDateStatic(period))
                          select c.Id).SingleOrDefault();

            Assert.Equal(1, custId);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Where_Nested_Static()
        {
            using var context = CreateContext();

            var custId = (from c in context.Customers
                          where c.Id
                              == UDFSqlContext.GetCustomerWithMostOrdersAfterDateStatic(
                                  UDFSqlContext.GetReportingPeriodStartDateStatic(
                                      UDFSqlContext.ReportingPeriod.Winter))
                          select c.Id).SingleOrDefault();

            Assert.Equal(1, custId);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Let_Correlated_Static()
        {
            using var context = CreateContext();

            var cust = (from c in context.Customers
                        let orderCount = UDFSqlContext.CustomerOrderCountStatic(c.Id)
                        where c.Id == 2
                        select new { c.LastName, OrderCount = orderCount }).Single();

            Assert.Equal("Two", cust.LastName);
            Assert.Equal(2, cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Let_Not_Correlated_Static()
        {
            using var context = CreateContext();

            var cust = (from c in context.Customers
                        let orderCount = UDFSqlContext.CustomerOrderCountStatic(2)
                        where c.Id == 2
                        select new { c.LastName, OrderCount = orderCount }).Single();

            Assert.Equal("Two", cust.LastName);
            Assert.Equal(2, cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Let_Not_Parameter_Static()
        {
            using var context = CreateContext();
            var customerId = 2;

            var cust = (from c in context.Customers
                        let orderCount = UDFSqlContext.CustomerOrderCountStatic(customerId)
                        where c.Id == customerId
                        select new { c.LastName, OrderCount = orderCount }).Single();

            Assert.Equal("Two", cust.LastName);
            Assert.Equal(2, cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Let_Nested_Static()
        {
            using var context = CreateContext();
            var customerId = 1;
            var starCount = 3;

            var cust = (from c in context.Customers
                        let orderCount = UDFSqlContext.StarValueStatic(starCount, UDFSqlContext.CustomerOrderCountStatic(customerId))
                        where c.Id == customerId
                        select new { c.LastName, OrderCount = orderCount }).Single();

            Assert.Equal("One", cust.LastName);
            Assert.Equal("***3", cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Unwind_Client_Eval_Where_Static()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 2 == UDFSqlContext.AddOneStatic(c.Id)
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Unwind_Client_Eval_OrderBy_Static()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       orderby UDFSqlContext.AddOneStatic(c.Id)
                       select c.Id).ToList());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Unwind_Client_Eval_Select_Static()
        {
            using var context = CreateContext();

            var results = (from c in context.Customers
                           orderby c.Id
                           select UDFSqlContext.AddOneStatic(c.Id)).ToList();

            Assert.Equal(4, results.Count);
            Assert.True(results.SequenceEqual(Enumerable.Range(2, 4)));
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Client_BCL_UDF_Static()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 2 == UDFSqlContext.AddOneStatic(Math.Abs(UDFSqlContext.CustomerOrderCountWithClientStatic(c.Id)))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Client_UDF_BCL_Static()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 2 == UDFSqlContext.AddOneStatic(UDFSqlContext.CustomerOrderCountWithClientStatic(Math.Abs(c.Id)))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_BCL_Client_UDF_Static()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 2 == Math.Abs(UDFSqlContext.AddOneStatic(UDFSqlContext.CustomerOrderCountWithClientStatic(c.Id)))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_BCL_UDF_Client_Static()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 1 == Math.Abs(UDFSqlContext.CustomerOrderCountWithClientStatic(UDFSqlContext.AddOneStatic(c.Id)))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_UDF_BCL_Client_Static()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 1 == UDFSqlContext.CustomerOrderCountWithClientStatic(Math.Abs(UDFSqlContext.AddOneStatic(c.Id)))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_UDF_Client_BCL_Static()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 1 == UDFSqlContext.CustomerOrderCountWithClientStatic(UDFSqlContext.AddOneStatic(Math.Abs(c.Id)))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Client_BCL_Static()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 3 == UDFSqlContext.AddOneStatic(Math.Abs(c.Id))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Client_UDF_Static()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 2 == UDFSqlContext.AddOneStatic(UDFSqlContext.CustomerOrderCountWithClientStatic(c.Id))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_BCL_Client_Static()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 3 == Math.Abs(UDFSqlContext.AddOneStatic(c.Id))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_BCL_UDF_Static()
        {
            using var context = CreateContext();

            var results = (from c in context.Customers
                           where 3 == Math.Abs(UDFSqlContext.CustomerOrderCountStatic(c.Id))
                           select c.Id).Single();

            Assert.Equal(1, results);
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_UDF_Client_Static()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 2 == UDFSqlContext.CustomerOrderCountWithClientStatic(UDFSqlContext.AddOneStatic(c.Id))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_UDF_BCL_Static()
        {
            using var context = CreateContext();

            var results = (from c in context.Customers
                           where 3 == UDFSqlContext.CustomerOrderCountStatic(Math.Abs(c.Id))
                           select c.Id).Single();

            Assert.Equal(1, results);
        }

        [ConditionalFact]
        public virtual void Nullable_navigation_property_access_preserves_schema_for_sql_function()
        {
            using var context = CreateContext();

            var result = context.Orders
                .OrderBy(o => o.Id)
                .Select(o => UDFSqlContext.IdentityString(o.Customer.FirstName))
                .FirstOrDefault();

            Assert.Equal("Customer", result);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_SqlFragment_Static()
        {
            using var context = CreateContext();

            var len = context.Customers.Count(c => c.LastName == UDFSqlContext.GetSqlFragmentStatic());

            Assert.Equal(1, len);
        }

        #endregion

        #region Instance

        [ConditionalFact]
        public virtual void Scalar_Function_Non_Static()
        {
            using var context = CreateContext();

            var custName = (from c in context.Customers
                            where c.Id == 1
                            select new { Id = context.StarValueInstance(4, c.Id), LastName = context.DollarValueInstance(2, c.LastName) })
                .Single();

            Assert.Equal("$$One", custName.LastName);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Extension_Method_Instance()
        {
            using var context = CreateContext();

            var len = context.Customers.Count(c => context.IsDateInstance(c.FirstName) == false);

            Assert.Equal(4, len);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_With_Translator_Translates_Instance()
        {
            using var context = CreateContext();
            var customerId = 3;

            var len = context.Customers.Where(c => c.Id == customerId)
                .Select(c => context.MyCustomLengthInstance(c.LastName)).Single();

            Assert.Equal(5, len);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_ClientEval_Method_As_Translateable_Method_Parameter_Instance()
        {
            using var context = CreateContext();

            Assert.Throws<NotImplementedException>(
                () => (from c in context.Customers
                       where c.Id == 1
                       select new { c.FirstName, OrderCount = context.CustomerOrderCountInstance(context.AddFiveInstance(c.Id - 5)) })
                    .Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Constant_Parameter_Instance()
        {
            using var context = CreateContext();
            var customerId = 1;

            var custs = context.Customers.Select(c => context.CustomerOrderCountInstance(customerId)).ToList();

            Assert.Equal(4, custs.Count);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Anonymous_Type_Select_Correlated_Instance()
        {
            using var context = CreateContext();

            var cust = (from c in context.Customers
                        where c.Id == 1
                        select new { c.LastName, OrderCount = context.CustomerOrderCountInstance(c.Id) }).Single();

            Assert.Equal("One", cust.LastName);
            Assert.Equal(3, cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Anonymous_Type_Select_Not_Correlated_Instance()
        {
            using var context = CreateContext();

            var cust = (from c in context.Customers
                        where c.Id == 1
                        select new { c.LastName, OrderCount = context.CustomerOrderCountInstance(1) }).Single();

            Assert.Equal("One", cust.LastName);
            Assert.Equal(3, cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Anonymous_Type_Select_Parameter_Instance()
        {
            using var context = CreateContext();
            var customerId = 1;

            var cust = (from c in context.Customers
                        where c.Id == customerId
                        select new { c.LastName, OrderCount = context.CustomerOrderCountInstance(customerId) }).Single();

            Assert.Equal("One", cust.LastName);
            Assert.Equal(3, cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Anonymous_Type_Select_Nested_Instance()
        {
            using var context = CreateContext();
            var customerId = 3;
            var starCount = 3;

            var cust = (from c in context.Customers
                        where c.Id == customerId
                        select new
                        {
                            c.LastName,
                            OrderCount = context.StarValueInstance(starCount, context.CustomerOrderCountInstance(customerId))
                        }).Single();

            Assert.Equal("Three", cust.LastName);
            Assert.Equal("***1", cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Where_Correlated_Instance()
        {
            using var context = CreateContext();

            var cust = (from c in context.Customers
                        where context.IsTopCustomerInstance(c.Id)
                        select c.Id.ToString().ToLower()).ToList();

            Assert.Single(cust);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Where_Not_Correlated_Instance()
        {
            using var context = CreateContext();
            var startDate = new DateTime(2000, 4, 1);

            var custId = (from c in context.Customers
                          where context.GetCustomerWithMostOrdersAfterDateInstance(startDate) == c.Id
                          select c.Id).SingleOrDefault();

            Assert.Equal(2, custId);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Where_Parameter_Instance()
        {
            using var context = CreateContext();
            var period = UDFSqlContext.ReportingPeriod.Winter;

            var custId = (from c in context.Customers
                          where c.Id
                              == context.GetCustomerWithMostOrdersAfterDateInstance(
                                  context.GetReportingPeriodStartDateInstance(period))
                          select c.Id).SingleOrDefault();

            Assert.Equal(1, custId);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Where_Nested_Instance()
        {
            using var context = CreateContext();

            var custId = (from c in context.Customers
                          where c.Id
                              == context.GetCustomerWithMostOrdersAfterDateInstance(
                                  context.GetReportingPeriodStartDateInstance(
                                      UDFSqlContext.ReportingPeriod.Winter))
                          select c.Id).SingleOrDefault();

            Assert.Equal(1, custId);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Let_Correlated_Instance()
        {
            using var context = CreateContext();

            var cust = (from c in context.Customers
                        let orderCount = context.CustomerOrderCountInstance(c.Id)
                        where c.Id == 2
                        select new { c.LastName, OrderCount = orderCount }).Single();

            Assert.Equal("Two", cust.LastName);
            Assert.Equal(2, cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Let_Not_Correlated_Instance()
        {
            using var context = CreateContext();

            var cust = (from c in context.Customers
                        let orderCount = context.CustomerOrderCountInstance(2)
                        where c.Id == 2
                        select new { c.LastName, OrderCount = orderCount }).Single();

            Assert.Equal("Two", cust.LastName);
            Assert.Equal(2, cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Let_Not_Parameter_Instance()
        {
            using var context = CreateContext();
            var customerId = 2;

            var cust = (from c in context.Customers
                        let orderCount = context.CustomerOrderCountInstance(customerId)
                        where c.Id == customerId
                        select new { c.LastName, OrderCount = orderCount }).Single();

            Assert.Equal("Two", cust.LastName);
            Assert.Equal(2, cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Let_Nested_Instance()
        {
            using var context = CreateContext();
            var customerId = 1;
            var starCount = 3;

            var cust = (from c in context.Customers
                        let orderCount = context.StarValueInstance(starCount, context.CustomerOrderCountInstance(customerId))
                        where c.Id == customerId
                        select new { c.LastName, OrderCount = orderCount }).Single();

            Assert.Equal("One", cust.LastName);
            Assert.Equal("***3", cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Unwind_Client_Eval_Where_Instance()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 2 == context.AddOneInstance(c.Id)
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Unwind_Client_Eval_OrderBy_Instance()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       orderby context.AddOneInstance(c.Id)
                       select c.Id).ToList());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Unwind_Client_Eval_Select_Instance()
        {
            using var context = CreateContext();

            var results = (from c in context.Customers
                           orderby c.Id
                           select context.AddOneInstance(c.Id)).ToList();

            Assert.Equal(4, results.Count);
            Assert.True(results.SequenceEqual(Enumerable.Range(2, 4)));
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Client_BCL_UDF_Instance()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 2 == context.AddOneInstance(Math.Abs(context.CustomerOrderCountWithClientInstance(c.Id)))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Client_UDF_BCL_Instance()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 2 == context.AddOneInstance(context.CustomerOrderCountWithClientInstance(Math.Abs(c.Id)))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_BCL_Client_UDF_Instance()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 2 == Math.Abs(context.AddOneInstance(context.CustomerOrderCountWithClientInstance(c.Id)))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_BCL_UDF_Client_Instance()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 1 == Math.Abs(context.CustomerOrderCountWithClientInstance(context.AddOneInstance(c.Id)))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_UDF_BCL_Client_Instance()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 1 == context.CustomerOrderCountWithClientInstance(Math.Abs(context.AddOneInstance(c.Id)))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_UDF_Client_BCL_Instance()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 1 == context.CustomerOrderCountWithClientInstance(context.AddOneInstance(Math.Abs(c.Id)))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Client_BCL_Instance()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 3 == context.AddOneInstance(Math.Abs(c.Id))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Client_UDF_Instance()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 2 == context.AddOneInstance(context.CustomerOrderCountWithClientInstance(c.Id))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_BCL_Client_Instance()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 3 == Math.Abs(context.AddOneInstance(c.Id))
                       select c.Id).Single());
        }

        public static Exception AssertThrows<T>(Func<object> testCode)
            where T : Exception, new()
        {
            testCode();

            return new T();
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_BCL_UDF_Instance()
        {
            using var context = CreateContext();
            var results = (from c in context.Customers
                           where 3 == Math.Abs(context.CustomerOrderCountInstance(c.Id))
                           select c.Id).Single();

            Assert.Equal(1, results);
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_UDF_Client_Instance()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 2 == context.CustomerOrderCountWithClientInstance(context.AddOneInstance(c.Id))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_UDF_BCL_Instance()
        {
            using var context = CreateContext();

            var results = (from c in context.Customers
                           where 3 == context.CustomerOrderCountInstance(Math.Abs(c.Id))
                           select c.Id).Single();

            Assert.Equal(1, results);
        }

        #endregion

        #endregion

        #region QueryableFunction

        [ConditionalFact]
        public virtual void QF_Owned_Many_Tracked_Select_Owned()
        {
            using (var context = CreateContext())
            {
                var query = (from c in context.Customers
                             from pi in context.GetPhoneInformation(c.Id, "234")
                             orderby c.Id
                             select new
                             {
                                 Customer = c
                             }).ToList();

                Assert.Equal(2, query.Count);
                Assert.Equal(2, query[0].Customer.Id);
                Assert.Equal(3, query[1].Customer.Id);
            }
        }

        [ConditionalFact]
        public virtual void QF_Owned_Many_NoTracking_Select_Owned()
        {
            using (var context = CreateContext())
            {
                var query = (from c in context.Customers
                             from pi in context.GetPhoneInformation(c.Id, "234")
                             orderby pi.Number
                             select pi).AsNoTracking().ToList();

                Assert.Equal(2, query.Count);
                Assert.Equal("234-789-2345", query[0].Number);
                Assert.Equal("234-873-4921", query[1].Number);
            }
        }

        [ConditionalFact]
        public virtual void QF_Owned_One_NoTracking_Select_Owned()
        {
            using (var context = CreateContext())
            {
                var query = (from c in context.Customers
                             from cc in context.GetCreditCards(c.Id)
                             orderby cc.Number
                             select cc).AsNoTracking().ToList();

                Assert.Equal(3, query.Count);

                Assert.Equal("123", query[0].Number);
                Assert.Equal(CreditCardType.BankOfDad, query[0].CreditCardType);

                Assert.Equal("554355", query[1].Number);
                Assert.Equal(CreditCardType.BuyNLarge, query[1].CreditCardType);

                Assert.Equal("99-99", query[2].Number);
                Assert.Equal(CreditCardType.ShutupAndTakeMyMoney, query[2].CreditCardType);
            }
        }

        [ConditionalFact]
        public virtual void QF_Owned_One_Tracked()
        {
            using (var context = CreateContext())
            {
                var query = (from c in context.Customers
                             from cc in context.GetCreditCards(c.Id)
                             orderby cc.Number
                             select new
                             {
                                 Customer = c,
                                 CreditCard = cc
                             }).ToList();

                Assert.Equal(3, query.Count);

                Assert.Equal(1, query[0].Customer.Id);
                Assert.Equal("123", query[0].CreditCard.Number);
                Assert.Equal(CreditCardType.BankOfDad, query[0].CreditCard.CreditCardType);

                Assert.Equal(3, query[1].Customer.Id);
                Assert.Equal("554355", query[1].CreditCard.Number);
                Assert.Equal(CreditCardType.BuyNLarge, query[1].CreditCard.CreditCardType);

                Assert.Equal(4, query[2].Customer.Id);
                Assert.Equal("99-99", query[2].CreditCard.Number);
                Assert.Equal(CreditCardType.ShutupAndTakeMyMoney, query[2].CreditCard.CreditCardType);
            }
        }

        [ConditionalFact(Skip = "Issue#15873")]
        public virtual void QF_Anonymous_Collection_No_PK_Throws()
        {
            using (var context = CreateContext())
            {
                var query = from c in context.Customers
                            select new { c.Id, products = context.GetTopSellingProductsForCustomer(c.Id).ToList() };

                //Assert.Contains(
                //    RelationalStrings.DbFunctionProjectedCollectionMustHavePK("GetTopSellingProductsForCustomer"),
                //    Assert.Throws<InvalidOperationException>(() => query.ToList()).Message);
            }
        }

        [ConditionalFact(Skip = "Issue#16314")]
        public virtual void QF_Anonymous_Collection_No_IQueryable_In_Projection_Throws()
        {
            using (var context = CreateContext())
            {
                var query = (from c in context.Customers
                             select new { c.Id, orders = context.GetCustomerOrderCountByYear(c.Id) });


                //Assert.Contains(
                //    RelationalStrings.DbFunctionCantProjectIQueryable(),
                //    Assert.Throws<InvalidOperationException>(() => query.ToList()).Message);
            }
        }

        [ConditionalFact]
        public virtual void QF_Stand_Alone()
        {
            using (var context = CreateContext())
            {
                var products = (from t in context.GetTopTwoSellingProducts()
                                orderby t.ProductId
                                select t).ToList();

                Assert.Equal(2, products.Count);
                Assert.Equal(3, products[0].ProductId);
                Assert.Equal(249, products[0].AmountSold);
                Assert.Equal(4, products[1].ProductId);
                Assert.Equal(184, products[1].AmountSold);
            }
        }

        [ConditionalFact]
        public virtual void QF_Stand_Alone_Parameter()
        {
            using (var context = CreateContext())
            {
                var orders = (from c in context.GetCustomerOrderCountByYear(1)
                              orderby c.Count descending
                              select c).ToList();

                Assert.Equal(2, orders.Count);
                Assert.Equal(2, orders[0].Count);
                Assert.Equal(2000, orders[0].Year);
                Assert.Equal(1, orders[1].Count);
                Assert.Equal(2001, orders[1].Year);
            }
        }

        [ConditionalFact]
        public virtual void QF_CrossApply_Correlated_Select_QF_Type()
        {
            using (var context = CreateContext())
            {
                var orders = (from c in context.Customers
                              from r in context.GetCustomerOrderCountByYear(c.Id)
                              orderby r.Year
                              select r
                             ).ToList();

                Assert.Equal(4, orders.Count);
                Assert.Equal(2, orders[0].Count);
                Assert.Equal(2, orders[1].Count);
                Assert.Equal(1, orders[2].Count);
                Assert.Equal(1, orders[3].Count);
                Assert.Equal(2000, orders[0].Year);
                Assert.Equal(2000, orders[1].Year);
                Assert.Equal(2001, orders[2].Year);
                Assert.Equal(2001, orders[3].Year);
            }
        }

        [ConditionalFact]
        public virtual void QF_CrossApply_Correlated_Select_Anonymous()
        {
            using (var context = CreateContext())
            {
                var orders = (from c in context.Customers
                              from r in context.GetCustomerOrderCountByYear(c.Id)
                              orderby c.Id, r.Year
                              select new
                              {
                                  c.Id,
                                  c.LastName,
                                  r.Year,
                                  r.Count
                              }).ToList();

                Assert.Equal(4, orders.Count);
                Assert.Equal(2, orders[0].Count);
                Assert.Equal(1, orders[1].Count);
                Assert.Equal(2, orders[2].Count);
                Assert.Equal(1, orders[3].Count);
                Assert.Equal(2000, orders[0].Year);
                Assert.Equal(2001, orders[1].Year);
                Assert.Equal(2000, orders[2].Year);
                Assert.Equal(2001, orders[3].Year);
                Assert.Equal(1, orders[0].Id);
                Assert.Equal(1, orders[1].Id);
                Assert.Equal(2, orders[2].Id);
                Assert.Equal(3, orders[3].Id);
            }
        }

        [ConditionalFact(Skip = "Issue#20184")]
        public virtual void QF_Select_Direct_In_Anonymous()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               select new
                               {
                                   c.Id,
                                   Prods = context.GetTopTwoSellingProducts().ToList(),
                               }).ToList();

                Assert.Equal(4, results.Count);
                Assert.Equal(2, results[0].Prods.Count);
                Assert.Equal(2, results[1].Prods.Count);
                Assert.Equal(2, results[2].Prods.Count);
                Assert.Equal(2, results[3].Prods.Count);
            }
        }

        [ConditionalFact]
        public virtual void QF_Select_Correlated_Direct_With_Function_Query_Parameter_Correlated_In_Anonymous()
        {
            using (var context = CreateContext())
            {
                var cust = (from c in context.Customers
                               where c.Id == 1
                               select new
                               {
                                   c.Id,
                                   Orders = context.GetOrdersWithMultipleProducts(context.AddValues(c.Id, 1)).ToList()
                               }).ToList();

                Assert.Single(cust);

                Assert.Equal(1, cust[0].Id);
                Assert.Equal(4, cust[0].Orders[0].OrderId);
                Assert.Equal(5, cust[0].Orders[1].OrderId);
                Assert.Equal(new DateTime(2000, 4, 21), cust[0].Orders[0].OrderDate);
                Assert.Equal(new DateTime(2000, 5, 20), cust[0].Orders[1].OrderDate);
            }
        }

        [ConditionalFact]
        public virtual void QF_Select_Correlated_Subquery_In_Anonymous()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               select new
                               {
                                   c.Id,
                                   OrderCountYear = context.GetOrdersWithMultipleProducts(c.Id).Where(o => o.OrderDate.Day == 21).ToList()
                               }).ToList();

                Assert.Equal(4, results.Count);
                Assert.Equal(1, results[0].Id);
                Assert.Equal(2, results[1].Id);
                Assert.Equal(3, results[2].Id);
                Assert.Equal(4, results[3].Id);
                Assert.Single(results[0].OrderCountYear);
                Assert.Single(results[1].OrderCountYear);
                Assert.Empty(results[2].OrderCountYear);
                Assert.Empty(results[3].OrderCountYear);
            }
        }

        [ConditionalFact]
        public virtual void QF_Select_Correlated_Subquery_In_Anonymous_Nested_With_QF()
        {
            using (var context = CreateContext())
            {
                var results = (from o in context.Orders
                                join osub in (from c in context.Customers
                                            from a in context.GetOrdersWithMultipleProducts(c.Id)
                                            select a.OrderId
                                    ) on o.Id equals osub
                                select new { o.CustomerId, o.OrderDate }).ToList();

                Assert.Equal(4, results.Count);

                Assert.Equal(1, results[0].CustomerId);
                Assert.Equal(new DateTime(2000, 1, 20), results[0].OrderDate);

                Assert.Equal(1, results[1].CustomerId);
                Assert.Equal(new DateTime(2000, 2, 21), results[1].OrderDate);

                Assert.Equal(2, results[2].CustomerId);
                Assert.Equal(new DateTime(2000, 4, 21), results[2].OrderDate);

                Assert.Equal(2, results[3].CustomerId);
                Assert.Equal(new DateTime(2000, 5, 20), results[3].OrderDate);
            }
        }

        [ConditionalFact(Skip = "Issue#20184")]
        public virtual void QF_Select_Correlated_Subquery_In_Anonymous_Nested()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               select new
                               {
                                   c.Id,
                                   OrderCountYear = context.GetOrdersWithMultipleProducts(c.Id).Where(o => o.OrderDate.Day == 21).Select(o => new
                                   {
                                       OrderCountYearNested = context.GetOrdersWithMultipleProducts(o.CustomerId).ToList(),
                                       Prods = context.GetTopTwoSellingProducts().ToList(),
                                   }).ToList()
                               }).ToList();

                Assert.Equal(4, results.Count);

                Assert.Single(results[0].OrderCountYear);
                Assert.Equal(2, results[0].OrderCountYear[0].Prods.Count);
                Assert.Equal(2, results[0].OrderCountYear[0].OrderCountYearNested.Count);

                Assert.Single(results[1].OrderCountYear);
                Assert.Equal(2, results[1].OrderCountYear[0].Prods.Count);
                Assert.Equal(2, results[1].OrderCountYear[0].OrderCountYearNested.Count);

                Assert.Empty(results[2].OrderCountYear);

                Assert.Empty(results[3].OrderCountYear);
            }
        }

        [ConditionalFact]
        public virtual void QF_Select_Correlated_Subquery_In_Anonymous_MultipleCollections()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               select new
                               {
                                   c.Id,
                                   Prods = context.GetTopTwoSellingProducts().Where(p => p.AmountSold == 249).Select(p => p.ProductId).ToList(),
                                   Addresses = c.Addresses.Where(a => a.State == "NY").ToList()
                               }).ToList();

                Assert.Equal(4, results.Count);
                Assert.Equal(3, results[0].Prods[0]);
                Assert.Equal(3, results[1].Prods[0]);
                Assert.Equal(3, results[2].Prods[0]);
                Assert.Equal(3, results[3].Prods[0]);

                Assert.Empty(results[0].Addresses);
                Assert.Equal("Apartment 5A, 129 West 81st Street", results[1].Addresses[0].Street);
                Assert.Equal("425 Grove Street, Apt 20", results[2].Addresses[0].Street);
                Assert.Empty(results[3].Addresses);
            }
        }

        [ConditionalFact]
        public virtual void QF_Select_NonCorrelated_Subquery_In_Anonymous()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               select new
                               {
                                   c.Id,
                                   Prods = context.GetTopTwoSellingProducts().Where(p => p.AmountSold == 249).Select(p => p.ProductId).ToList(),
                               }).ToList();

                Assert.Equal(4, results.Count);
                Assert.Equal(3, results[0].Prods[0]);
                Assert.Equal(3, results[1].Prods[0]);
                Assert.Equal(3, results[2].Prods[0]);
                Assert.Equal(3, results[3].Prods[0]);
            }
        }

        [ConditionalFact]
        public virtual void QF_Select_NonCorrelated_Subquery_In_Anonymous_Parameter()
        {
            using (var context = CreateContext())
            {
                var amount = 27;

                var results = (from c in context.Customers
                               select new
                               {
                                   c.Id,
                                   Prods = context.GetTopTwoSellingProducts().Where(p => p.AmountSold == amount).Select(p => p.ProductId).ToList(),
                               }).ToList();

                Assert.Equal(4, results.Count);
                Assert.Single(results[0].Prods);
                Assert.Single(results[1].Prods);
                Assert.Single(results[2].Prods);
                Assert.Single(results[3].Prods);
            }
        }

        [ConditionalFact]
        public virtual void QF_Correlated_Select_In_Anonymous()
        {
            using (var context = CreateContext())
            {
               var cust = (from c in context.Customers
                            orderby c.Id
                            select new
                            {
                                c.Id,
                                c.LastName,
                                Orders = context.GetOrdersWithMultipleProducts(c.Id).ToList()
                            }).ToList();

                Assert.Equal(4, cust.Count);

                Assert.Equal(1, cust[0].Id);
                Assert.Equal(2, cust[0].Orders.Count);
                Assert.Equal(1, cust[0].Orders[0].OrderId);
                Assert.Equal(2, cust[0].Orders[1].OrderId);
                Assert.Equal(new DateTime(2000, 1, 20), cust[0].Orders[0].OrderDate);
                Assert.Equal(new DateTime(2000, 2, 21), cust[0].Orders[1].OrderDate);

                Assert.Equal(2, cust[1].Id);
                Assert.Equal(2, cust[1].Orders.Count);
                Assert.Equal(4, cust[1].Orders[0].OrderId);
                Assert.Equal(5, cust[1].Orders[1].OrderId);
                Assert.Equal(new DateTime(2000, 4, 21), cust[1].Orders[0].OrderDate);
                Assert.Equal(new DateTime(2000, 5, 20), cust[1].Orders[1].OrderDate);

                Assert.Equal(3, cust[2].Id);
                Assert.Empty(cust[2].Orders);

                Assert.Equal(4, cust[3].Id);
                Assert.Empty(cust[3].Orders);
            }
        }

        [ConditionalFact]
        public virtual void QF_CrossApply_Correlated_Select_Result()
        {
            using (var context = CreateContext())
            {
                var orders = (from c in context.Customers
                              from r in context.GetCustomerOrderCountByYear(c.Id)
                              orderby r.Count descending, r.Year descending
                              select r).ToList();

                Assert.Equal(4, orders.Count);

                Assert.Equal(4, orders.Count);
                Assert.Equal(2, orders[0].Count);
                Assert.Equal(2, orders[1].Count);
                Assert.Equal(1, orders[2].Count);
                Assert.Equal(1, orders[3].Count);
                Assert.Equal(2000, orders[0].Year);
                Assert.Equal(2000, orders[1].Year);
                Assert.Equal(2001, orders[2].Year);
                Assert.Equal(2001, orders[3].Year);
            }
        }

        [ConditionalFact]
        public virtual void QF_CrossJoin_Not_Correlated()
        {
            using (var context = CreateContext())
            {
                var orders = (from c in context.Customers
                              from r in context.GetCustomerOrderCountByYear(2)
                              where c.Id == 2
                              orderby r.Count
                              select new
                              {
                                  c.Id,
                                  c.LastName,
                                  r.Year,
                                  r.Count
                              }).ToList();

                Assert.Single(orders);

                Assert.Equal(2, orders[0].Count);
                Assert.Equal(2000, orders[0].Year);
            }
        }

        [ConditionalFact]
        public virtual void QF_CrossJoin_Parameter()
        {
            using (var context = CreateContext())
            {
                var custId = 2;

                var orders = (from c in context.Customers
                              from r in context.GetCustomerOrderCountByYear(custId)
                              where c.Id == custId
                              orderby r.Count
                              select new
                              {
                                  c.Id,
                                  c.LastName,
                                  r.Year,
                                  r.Count
                              }).ToList();

                Assert.Single(orders);

                Assert.Equal(2, orders[0].Count);
                Assert.Equal(2000, orders[0].Year);
            }
        }

        [ConditionalFact]
        public virtual void QF_Join()
        {
            using (var context = CreateContext())
            {
                var products = (from p in context.Products
                                join r in context.GetTopTwoSellingProducts() on p.Id equals r.ProductId
                                select new
                                {
                                    p.Id,
                                    p.Name,
                                    r.AmountSold
                                }).OrderBy(p => p.Id).ToList();

                Assert.Equal(2, products.Count);
                Assert.Equal(3, products[0].Id);
                Assert.Equal("Product3", products[0].Name);
                Assert.Equal(249, products[0].AmountSold);
                Assert.Equal(4, products[1].Id);
                Assert.Equal("Product4", products[1].Name);
                Assert.Equal(184, products[1].AmountSold);
            }
        }

        [ConditionalFact]
        public virtual void QF_LeftJoin_Select_Anonymous()
        {
            using (var context = CreateContext())
            {
                var products = (from p in context.Products
                                join r in context.GetTopTwoSellingProducts() on p.Id equals r.ProductId into joinTable
                                from j in joinTable.DefaultIfEmpty()
                                orderby p.Id descending
                                select new
                                {
                                    p.Id,
                                    p.Name,
                                    j.AmountSold
                                }).ToList();

                Assert.Equal(5, products.Count);
                Assert.Equal(5, products[0].Id);
                Assert.Equal("Product5", products[0].Name);
                Assert.Null(products[0].AmountSold);

                Assert.Equal(4, products[1].Id);
                Assert.Equal("Product4", products[1].Name);
                Assert.Equal(184, products[1].AmountSold);

                Assert.Equal(3, products[2].Id);
                Assert.Equal("Product3", products[2].Name);
                Assert.Equal(249, products[2].AmountSold);

                Assert.Equal(2, products[3].Id);
                Assert.Equal("Product2", products[3].Name);
                Assert.Null(products[3].AmountSold);

                Assert.Equal(1, products[4].Id);
                Assert.Equal("Product1", products[4].Name);
                Assert.Null(products[4].AmountSold);
            }
        }

        [ConditionalFact]
        public virtual void QF_LeftJoin_Select_Result()
        {
            using (var context = CreateContext())
            {
                var products = (from p in context.Products
                                join r in context.GetTopTwoSellingProducts() on p.Id equals r.ProductId into joinTable
                                from j in joinTable.DefaultIfEmpty()
                                orderby p.Id descending
                                select j).ToList();

                Assert.Equal(5, products.Count);
                Assert.Null(products[0]);
                Assert.Equal(4, products[1].ProductId);
                Assert.Equal(184, products[1].AmountSold);
                Assert.Equal(3, products[2].ProductId);
                Assert.Equal(249, products[2].AmountSold);
                Assert.Null(products[3]);
                Assert.Null(products[4]);
            }
        }

        [ConditionalFact]
        public virtual void QF_OuterApply_Correlated_Select_QF()
        {
            using (var context = CreateContext())
            {
                var orders = (from c in context.Customers
                              from r in context.GetCustomerOrderCountByYear(c.Id).DefaultIfEmpty()
                              orderby c.Id, r.Year
                              select r).ToList();

                Assert.Equal(5, orders.Count);

                Assert.Equal(2, orders[0].Count);
                Assert.Equal(1, orders[1].Count);
                Assert.Equal(2, orders[2].Count);
                Assert.Equal(1, orders[3].Count);
                Assert.Null(orders[4]);
                Assert.Equal(2000, orders[0].Year);
                Assert.Equal(2001, orders[1].Year);
                Assert.Equal(2000, orders[2].Year);
                Assert.Equal(2001, orders[3].Year);
                Assert.Null(orders[4]);
                Assert.Equal(1, orders[0].CustomerId);
                Assert.Equal(1, orders[1].CustomerId);
                Assert.Equal(2, orders[2].CustomerId);
                Assert.Equal(3, orders[3].CustomerId);
                Assert.Null(orders[4]);
            }
        }

        [ConditionalFact]
        public virtual void QF_OuterApply_Correlated_Select_Entity()
        {
            using (var context = CreateContext())
            {
                var custs = (from c in context.Customers
                             from r in context.GetCustomerOrderCountByYear(c.Id).DefaultIfEmpty()
                             where r.Year == 2000
                             orderby c.Id, r.Year
                             select c).ToList();

                Assert.Equal(2, custs.Count);

                Assert.Equal(1, custs[0].Id);
                Assert.Equal(2, custs[1].Id);
                Assert.Equal("One", custs[0].LastName);
                Assert.Equal("Two", custs[1].LastName);
            }
        }

        [ConditionalFact]
        public virtual void QF_OuterApply_Correlated_Select_Anonymous()
        {
            using (var context = CreateContext())
            {
                var orders = (from c in context.Customers
                              from r in context.GetCustomerOrderCountByYear(c.Id).DefaultIfEmpty()
                              orderby c.Id, r.Year
                              select new
                              {
                                  c.Id,
                                  c.LastName,
                                  r.Year,
                                  r.Count
                              }).ToList();

                Assert.Equal(5, orders.Count);

                Assert.Equal(1, orders[0].Id);
                Assert.Equal(1, orders[1].Id);
                Assert.Equal(2, orders[2].Id);
                Assert.Equal(3, orders[3].Id);
                Assert.Equal(4, orders[4].Id);
                Assert.Equal("One", orders[0].LastName);
                Assert.Equal("One", orders[1].LastName);
                Assert.Equal("Two", orders[2].LastName);
                Assert.Equal("Three", orders[3].LastName);
                Assert.Equal("Four", orders[4].LastName);
                Assert.Equal(2, orders[0].Count);
                Assert.Equal(1, orders[1].Count);
                Assert.Equal(2, orders[2].Count);
                Assert.Equal(1, orders[3].Count);
                Assert.Null(orders[4].Count);
                Assert.Equal(2000, orders[0].Year);
                Assert.Equal(2001, orders[1].Year);
                Assert.Equal(2000, orders[2].Year);
                Assert.Equal(2001, orders[3].Year);
            }
        }

        [ConditionalFact]
        public virtual void QF_Nested()
        {
            using (var context = CreateContext())
            {
                var custId = 2;

                var orders = (from c in context.Customers
                              from r in context.GetCustomerOrderCountByYear(context.AddValues(1, 1))
                              where c.Id == custId
                              orderby r.Year
                              select new
                              {
                                  c.Id,
                                  c.LastName,
                                  r.Year,
                                  r.Count
                              }).ToList();

                Assert.Single(orders);

                Assert.Equal(2, orders[0].Count);
                Assert.Equal(2000, orders[0].Year);
            }
        }


        [ConditionalFact]
        public virtual void QF_Correlated_Nested_Func_Call()
        {
            var custId = 2;

            using (var context = CreateContext())
            {
                var orders = (from c in context.Customers
                              from r in context.GetCustomerOrderCountByYear(context.AddValues(c.Id, 1))
                              where c.Id == custId
                              select new
                              {
                                  c.Id,
                                  r.Count,
                                  r.Year
                              }).ToList();

                Assert.Single(orders);

                Assert.Equal(1, orders[0].Count);
                Assert.Equal(2001, orders[0].Year);
            }
        }

        [ConditionalFact]
        public virtual void QF_Correlated_Func_Call_With_Navigation()
        {
            using (var context = CreateContext())
            {
                var cust = (from c in context.Customers
                            orderby c.Id
                            select new
                            {
                                c.Id,
                                Orders = context.GetOrdersWithMultipleProducts(c.Id).Select(mpo => new
                                {
                                    //how to I setup the PK/FK combo properly for this?  Is it even possible?
                                    //OrderName = mpo.Order.Name,
                                    CustomerName = mpo.Customer.LastName
                                }).ToList()
                            }).ToList();

                Assert.Equal(4, cust.Count);
                Assert.Equal(2, cust[0].Orders.Count);
                Assert.Equal("One", cust[0].Orders[0].CustomerName);
                Assert.Equal(2, cust[1].Orders.Count);
                Assert.Equal("Two", cust[1].Orders[0].CustomerName);
            }
        }

    #endregion

    private void AssertTranslationFailed(Action testCode)
        => Assert.Contains(
            CoreStrings.TranslationFailed("").Substring(21),
            Assert.Throws<InvalidOperationException>(testCode).Message);
    }
}
