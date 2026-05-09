using IMSWEB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace IMSWEB.Data
{
    public static class SalesOrderExtensions
    {
        public static async Task<IEnumerable<Tuple<int, string, DateTime, string,
           string, decimal, EnumSalesType, Tuple<string>>>> GetAllSalesOrderAsync(this IBaseRepository<SOrder> salesOrderRepository,
           IBaseRepository<Customer> customerRepository, IBaseRepository<SisterConcern> SisterConcernRepository,
           DateTime fromDate, DateTime toDate, List<EnumSalesType> SalesType, bool IsVATManager, int concernID,
           string InvoiceNo, string ContactNo, string CustomerName, string AccountNo)
        {
            IQueryable<Customer> customers = customerRepository.All;
            IQueryable<SOrder> sorders = salesOrderRepository.All.Where(x => x.IsReplacement == 0 && SalesType.Contains((EnumSalesType)x.Status));
            bool IsSearchByDate = true;
            if (!string.IsNullOrWhiteSpace(InvoiceNo))
            {
                sorders = sorders.Where(i => i.InvoiceNo.Contains(InvoiceNo));
                IsSearchByDate = false;
            }
            if (!string.IsNullOrWhiteSpace(ContactNo))
            {
                customers = customers.Where(i => i.ContactNo.Contains(ContactNo));
                IsSearchByDate = false;
            }
            if (!string.IsNullOrWhiteSpace(CustomerName))
            {
                customers = customers.Where(i => i.Name.Contains(CustomerName));
                IsSearchByDate = false;
            }

            if (!string.IsNullOrWhiteSpace(AccountNo))
            {
                customers = customers.Where(i => i.Code.Contains(AccountNo));
                IsSearchByDate = false;
            }

            if (IsSearchByDate)
                sorders = sorders.Where(i => (i.InvoiceDate >= fromDate && i.InvoiceDate <= toDate));


            var items = await (from so in sorders
                               join c in customers on so.CustomerID equals c.CustomerID
                               select new ProductWiseSalesReportModel
                               {
                                   SOrderID = so.SOrderID,
                                   InvoiceNo = so.InvoiceNo,
                                   Date = so.InvoiceDate,
                                   CustomerName = c.Name,
                                   //CustomerCode = c.Code,
                                   Mobile = c.ContactNo,
                                   TotalDue = so.PaymentDue,
                                   Status = so.Status,
                                   IsReplacement = so.IsReplacement,
                                   //TotalAmount = so.TotalAmount
                               }).ToListAsync();

            List<ProductWiseSalesReportModel> finalData = new List<ProductWiseSalesReportModel>();
            if (IsVATManager)
            {
                items = items.OrderByDescending(i => i.Date).ToList();
                var oConcern = SisterConcernRepository.All.FirstOrDefault(i => i.ConcernID == concernID);
                decimal FalesSales = (items.Sum(i => i.TotalAmount) * oConcern.SalesShowPercent) / 100m;
                decimal FalesSalesCount = 0m;


                foreach (var item in items)
                {
                    FalesSalesCount += item.TotalAmount;
                    if (FalesSalesCount <= FalesSales)
                        finalData.Add(item);
                    else
                        break;
                }
            }
            else
                finalData = items;

            return finalData.Select(x => new Tuple<int, string, DateTime, string, string, decimal, EnumSalesType, Tuple<string>>
                (
                    x.SOrderID,
                    x.InvoiceNo,
                    x.Date,
                    x.CustomerName,
                    x.Mobile,
                    x.TotalDue,
                    (EnumSalesType)x.Status,
                    new Tuple<string>
                    (x.CustomerCode)
                )).OrderByDescending(x => x.Item3).ThenByDescending(i => i.Item2).ToList();
        }


        public static async Task<IEnumerable<Tuple<int, string, DateTime, string, string, decimal, EnumSalesType>>>
            GetAllSalesOrderAsync(this IBaseRepository<SOrder> salesOrderRepository,
                                 IBaseRepository<Customer> customerRepository, DateTime fromDate, DateTime toDate,
                                 EnumSalesType SalesType, int ConcernID)
        {
            IQueryable<Customer> customers = customerRepository.GetAll().Where(i => i.ConcernID == ConcernID);

            var items = await salesOrderRepository.GetAll().Where(i => i.ConcernID == ConcernID).
                GroupJoin(customers, p => p.CustomerID, c => c.CustomerID,
                (p, c) => new { SalesOrder = p, Customers = c }).
                SelectMany(x => x.Customers.DefaultIfEmpty(), (p, c) => new { SalesOrder = p.SalesOrder, Customer = c })
                .Where(x => (x.SalesOrder.InvoiceDate >= fromDate && x.SalesOrder.InvoiceDate <= toDate) && x.SalesOrder.Status == (int)SalesType)
                .Select(x => new
                {
                    x.SalesOrder.SOrderID,
                    x.SalesOrder.InvoiceNo,
                    x.SalesOrder.InvoiceDate,
                    x.Customer.Name,
                    x.Customer.ContactNo,
                    x.Customer.TotalDue,
                    x.SalesOrder.Status,
                    x.SalesOrder.IsReplacement
                }).Where(i => i.IsReplacement == 0).ToListAsync();

            return items.Select(x => new Tuple<int, string, DateTime, string, string, decimal, EnumSalesType>
                (
                    x.SOrderID,
                    x.InvoiceNo,
                    x.InvoiceDate,
                    x.Name,
                    x.ContactNo,
                    x.TotalDue,
                    (EnumSalesType)x.Status
                )).OrderByDescending(x => x.Item3).ThenByDescending(i => i.Item2).ToList();
        }
        public static async Task<IEnumerable<Tuple<int, string, DateTime, string,
            string, decimal, EnumSalesType, Tuple<string>>>> GetAllSalesOrderAsyncByUserID(this IBaseRepository<SOrder> salesOrderRepository,
            IBaseRepository<Customer> customerRepository, int UserID,
            DateTime fromDate, DateTime toDate, EnumSalesType SalesType,
            string InvoiceNo, string ContactNo, string CustomerName, string AccountNo)
        {
            IQueryable<Customer> customers = customerRepository.All;
            IQueryable<SOrder> sorders = salesOrderRepository.All
                                        .Where(x => x.Status == (int)SalesType && x.CreatedBy == UserID);

            bool IsSearchByDate = true;
            if (!string.IsNullOrWhiteSpace(InvoiceNo))
            {
                sorders = sorders.Where(i => i.InvoiceNo.Contains(InvoiceNo));
                IsSearchByDate = false;
            }
            if (!string.IsNullOrWhiteSpace(ContactNo))
            {
                customers = customers.Where(i => i.ContactNo.Contains(ContactNo));
                IsSearchByDate = false;
            }
            if (!string.IsNullOrWhiteSpace(CustomerName))
            {
                customers = customers.Where(i => i.Name.Contains(CustomerName));
                IsSearchByDate = false;
            }

            if (!string.IsNullOrWhiteSpace(AccountNo))
            {
                customers = customers.Where(i => i.Name.Contains(AccountNo));
                IsSearchByDate = false;
            }

            if (IsSearchByDate)
                sorders = sorders.Where(i => (i.InvoiceDate >= fromDate && i.InvoiceDate <= toDate));

            var items = await salesOrderRepository.All.
                GroupJoin(customers, p => p.CustomerID, c => c.CustomerID,
                (p, c) => new { SalesOrder = p, Customers = c }).
                SelectMany(x => x.Customers.DefaultIfEmpty(), (p, c) => new { SalesOrder = p.SalesOrder, Customer = c })
                .Select(x => new
                {
                    x.SalesOrder.SOrderID,
                    x.SalesOrder.InvoiceNo,
                    x.SalesOrder.InvoiceDate,
                    x.Customer.Code,
                    x.Customer.Name,
                    x.Customer.ContactNo,
                    x.Customer.TotalDue,
                    x.SalesOrder.Status,
                    x.SalesOrder.CreatedBy,
                    x.SalesOrder.IsReplacement
                }).Where(i => i.IsReplacement == 0).OrderByDescending(i => i.InvoiceDate).ToListAsync();

            return items.Select(x => new Tuple<int, string, DateTime, string, string, decimal, EnumSalesType, Tuple<string>>
                (
                    x.SOrderID,
                    x.InvoiceNo,
                    x.InvoiceDate,
                    x.Name,
                    x.ContactNo,
                    x.TotalDue,
                    (EnumSalesType)x.Status,
                   new Tuple<string>
                    (x.Code)

                )).ToList();
        }


        public static IEnumerable<SOredersReportModel> GetforSalesReport(
            this IBaseRepository<SOrder> salesOrderRepository, IBaseRepository<Customer> customerRepository,
            IBaseRepository<Employee> EmployeeRepository,
            DateTime fromDate, DateTime toDate, int EmployeeID, int CustomerID)
        {
            IQueryable<Customer> Customers = null;
            if (CustomerID > 0)
                Customers = customerRepository.All.Where(i => i.CustomerID == CustomerID);
            else if (EmployeeID > 0)
                Customers = customerRepository.All.Where(i => i.EmployeeID == EmployeeID);
            else
                Customers = customerRepository.All;

            var oSalesData = (from sord in salesOrderRepository.All
                              join cus in Customers on sord.CustomerID equals cus.CustomerID
                              join emp in EmployeeRepository.All on cus.EmployeeID equals emp.EmployeeID
                              where (sord.InvoiceDate >= fromDate && sord.InvoiceDate <= toDate) && sord.Status == (int)EnumSalesType.Sales && sord.RecAmount > 0m
                              select new SOredersReportModel
                              {
                                  CustomerCode = cus.Code,
                                  CustomerName = cus.Name,
                                  InvoiceDate = sord.InvoiceDate,
                                  InvoiceNo = sord.InvoiceNo,
                                  Grandtotal = sord.GrandTotal,
                                  FlatDiscount = sord.TDAmount,
                                  TotalAmount = sord.TotalAmount,
                                  RecAmount = (decimal)sord.RecAmount,
                                  PaymentDue = sord.PaymentDue,
                                  CustomerID = sord.CustomerID,
                                  CustomerTotalDue = cus.TotalDue,
                                  EmployeeName = emp.Name,
                              }).ToList();
            return oSalesData;
        }

        public static IEnumerable<Tuple<DateTime, string, string, decimal, decimal, decimal, decimal, Tuple<decimal, decimal, decimal, decimal, decimal, string>>>
            GetforSalesDetailReport(this IBaseRepository<SOrder> salesOrderRepository, IBaseRepository<SOrderDetail> SorderDetailRepository, IBaseRepository<Product> productRepository,
            IBaseRepository<StockDetail> stockdetailRepository, DateTime fromDate, DateTime toDate)
        {
            var oSalesDetailData = (from SOD in SorderDetailRepository.All
                                    from SO in salesOrderRepository.All
                                    from P in productRepository.All
                                    from std in stockdetailRepository.All
                                    where (SOD.SOrderID == SO.SOrderID && SOD.SDetailID == std.SDetailID && P.ProductID == SOD.ProductID && SO.InvoiceDate >= fromDate && SO.InvoiceDate <= toDate && SO.Status == 1)
                                    select new { SO.InvoiceNo, SO.InvoiceDate, SO.GrandTotal, SO.TDAmount, SO.TotalAmount, SO.RecAmount, SO.PaymentDue, P.ProductID, P.ProductName, SOD.UnitPrice, SOD.UTAmount, SOD.PPDAmount, SOD.Quantity, std.IMENO }).OrderByDescending(x => x.InvoiceDate).ToList();

            return oSalesDetailData.Select(x => new Tuple<DateTime, string, string, decimal, decimal, decimal, decimal, Tuple<decimal, decimal, decimal, decimal, decimal, string>>
                (
                 x.InvoiceDate,
                 x.InvoiceNo,
                x.ProductName,
                x.UnitPrice,
                x.PPDAmount,
                x.UTAmount,
                x.GrandTotal, new Tuple<decimal, decimal, decimal, decimal, decimal, string>(
                                    x.TDAmount,
                                    x.TotalAmount,
                                   (decimal)x.RecAmount,
                                   x.PaymentDue,
                                   x.Quantity,
                                   x.IMENO)
                ));
        }

        public static IEnumerable<SOredersReportModel>
         GetforSalesDetailReportByMO(this IBaseRepository<SOrder> salesOrderRepository, IBaseRepository<SOrderDetail> SorderDetailRepository, IBaseRepository<Product> productRepository,
         IBaseRepository<StockDetail> stockdetailRepository, IBaseRepository<Customer> customerRepository, IBaseRepository<Employee> employeeRepository, DateTime fromDate, DateTime toDate, int MOId)
        {
            var oSalesDetailData = (from SOD in SorderDetailRepository.All
                                    join SO in salesOrderRepository.All on SOD.SOrderID equals SO.SOrderID
                                    join P in productRepository.All on SOD.ProductID equals P.ProductID
                                    join std in stockdetailRepository.All on SOD.SDetailID equals std.SDetailID
                                    join CO in customerRepository.All on SO.CustomerID equals CO.CustomerID
                                    join emp in employeeRepository.All on CO.EmployeeID equals emp.EmployeeID
                                    //where (CO.CustomerID == SO.CustomerID && SOD.SOrderID == SO.SOrderID && SOD.SDetailID == std.SDetailID && P.ProductID == SOD.ProductID && SO.InvoiceDate >= fromDate && SO.InvoiceDate <= toDate && CO.EmployeeID == emp.EmployeeID && CO.EmployeeID == MOId && SO.Status ==(int) EnumSalesType.Sales)
                                    where (SO.InvoiceDate >= fromDate && SO.InvoiceDate <= toDate && CO.EmployeeID == MOId && SO.Status == (int)EnumSalesType.Sales && SO.IsReplacement != 1)
                                    select new SOredersReportModel
                                    {
                                        SOrderID = SO.SOrderID,
                                        EmployeeName = emp.Name,
                                        CustomerCode = CO.Code,
                                        CustomerName = CO.Name,
                                        InvoiceNo = SO.InvoiceNo,
                                        InvoiceDate = SO.InvoiceDate,
                                        TotalAmount = SO.TotalAmount,
                                        NetDiscount = SO.NetDiscount,
                                        AdjAmount = SO.AdjAmount,
                                        RecAmount = (decimal)SO.RecAmount,
                                        PaymentDue = SO.PaymentDue,
                                        ProductName = P.ProductName,
                                        UnitPrice = SOD.UnitPrice,
                                        PPDAmount = SOD.PPDAmount,
                                        Quantity = SOD.Quantity,
                                        IMENO = std.IMENO,
                                        Grandtotal = SO.GrandTotal
                                    }).OrderByDescending(x => x.SOrderID).ToList();

            var Replacements = (from SO in salesOrderRepository.All
                                join SOD in SorderDetailRepository.All on SO.SOrderID equals SOD.RepOrderID
                                join P in productRepository.All on SOD.ProductID equals P.ProductID
                                join std in stockdetailRepository.All on SOD.RStockDetailID equals std.SDetailID
                                join CO in customerRepository.All on SO.CustomerID equals CO.CustomerID
                                join emp in employeeRepository.All on CO.EmployeeID equals emp.EmployeeID
                                //where (CO.CustomerID == SO.CustomerID && SOD.SOrderID == SO.SOrderID && SOD.SDetailID == std.SDetailID && P.ProductID == SOD.ProductID && SO.InvoiceDate >= fromDate && SO.InvoiceDate <= toDate && CO.EmployeeID == emp.EmployeeID && CO.EmployeeID == MOId && SO.Status ==(int) EnumSalesType.Sales)
                                where (SO.InvoiceDate >= fromDate && SO.InvoiceDate <= toDate && CO.EmployeeID == MOId && SO.Status == (int)EnumSalesType.Sales && SO.IsReplacement == 1)
                                select new SOredersReportModel
                                {
                                    SOrderID = SO.SOrderID,
                                    EmployeeName = emp.Name,
                                    CustomerCode = CO.Code,
                                    CustomerName = CO.Name,
                                    InvoiceNo = "REP-" + SO.InvoiceNo,
                                    InvoiceDate = SO.InvoiceDate,
                                    TotalAmount = SO.TotalAmount,
                                    NetDiscount = SO.NetDiscount,
                                    AdjAmount = SO.AdjAmount,
                                    RecAmount = (decimal)SO.RecAmount,
                                    PaymentDue = SO.PaymentDue,
                                    ProductName = P.ProductName,
                                    UnitPrice = (decimal)SOD.RepUnitPrice,
                                    PPDAmount = SOD.PPDAmount,
                                    Quantity = SOD.Quantity,
                                    IMENO = std.IMENO,
                                    Grandtotal = SO.GrandTotal
                                }).OrderByDescending(x => x.SOrderID).ToList();

            oSalesDetailData.AddRange(Replacements);
            return oSalesDetailData;

        }


        public static IEnumerable<Tuple<string, string, DateTime, string, decimal, decimal, decimal, Tuple<decimal, decimal, decimal, decimal>>>
            GetSalesReportByConcernID(this IBaseRepository<SOrder> salesOrderRepository, IBaseRepository<Customer> customerRepository,
            IBaseRepository<SOrderDetail> SOrderDetailRepsitory,
            DateTime fromDate, DateTime toDate, int concernID, int CustomerType)
        {
            IQueryable<Customer> Customers = null;
            if (CustomerType == 0)
                Customers = customerRepository.All;
            else
                Customers = customerRepository.All.Where(i => i.CustomerType == (EnumCustomerType)CustomerType);

            var oSalesData = (from SO in salesOrderRepository.All
                              join SOD in SOrderDetailRepsitory.All on SO.SOrderID equals SOD.SOrderID
                              join cus in Customers on SO.CustomerID equals cus.CustomerID
                              where (SO.InvoiceDate >= fromDate && SO.InvoiceDate <= toDate && SO.Status == (int)EnumSalesType.Sales && SO.IsReplacement != 1)
                              group SO by new
                              {
                                  cus.Code,
                                  cus.Name,
                                  SO.InvoiceNo,
                                  SO.InvoiceDate,
                                  SO.GrandTotal,
                                  SO.NetDiscount,
                                  SO.TotalAmount,
                                  SO.RecAmount,
                                  SO.PaymentDue,
                                  SO.AdjAmount,

                              } into g
                              select new
                              {
                                  g.Key.Code,
                                  g.Key.Name,
                                  g.Key.InvoiceDate,
                                  g.Key.InvoiceNo,
                                  g.Key.GrandTotal,
                                  g.Key.NetDiscount,
                                  g.Key.TotalAmount,
                                  g.Key.RecAmount,
                                  g.Key.PaymentDue,
                                  g.Key.AdjAmount,
                                  TotalOffer = g.Select(i => i.SOrderDetails).FirstOrDefault()
                              }).ToList();

            var Replacements = (from SO in salesOrderRepository.All
                                join SOD in SOrderDetailRepsitory.All on SO.SOrderID equals SOD.RepOrderID
                                join cus in Customers on SO.CustomerID equals cus.CustomerID
                                where (SO.InvoiceDate >= fromDate && SO.InvoiceDate <= toDate && SO.Status == (int)EnumSalesType.Sales && SO.IsReplacement == 1)
                                group SO by new
                                {
                                    cus.Code,
                                    cus.Name,
                                    InvoiceNo = "REP-" + SO.InvoiceNo,
                                    SO.InvoiceDate,
                                    SO.GrandTotal,
                                    SO.NetDiscount,
                                    SO.TotalAmount,
                                    SO.RecAmount,
                                    SO.PaymentDue,
                                    SO.AdjAmount,

                                } into g
                                select new
                                {
                                    g.Key.Code,
                                    g.Key.Name,
                                    g.Key.InvoiceDate,
                                    g.Key.InvoiceNo,
                                    g.Key.GrandTotal,
                                    g.Key.NetDiscount,
                                    g.Key.TotalAmount,
                                    g.Key.RecAmount,
                                    g.Key.PaymentDue,
                                    g.Key.AdjAmount,
                                    TotalOffer = g.Select(i => i.SOrderDetails).FirstOrDefault()
                                }).ToList();

            oSalesData.AddRange(Replacements);

            return oSalesData.Select(x => new Tuple<string, string, DateTime, string, decimal, decimal, decimal, Tuple<decimal, decimal, decimal, decimal>>
                (
                 x.Code,
                 x.Name,
                x.InvoiceDate,
                                    x.InvoiceNo,
                                    x.GrandTotal,
                                    x.NetDiscount,
                                    x.TotalAmount,
                                     new Tuple<decimal, decimal, decimal, decimal>(
                                    (decimal)x.RecAmount,
                                    x.PaymentDue,
                                    x.AdjAmount,
                                    x.TotalOffer.Sum(i => i.PPOffer)
                                    )

                ));
        }

        public static IEnumerable<ProductWiseSalesReportModel> GetSalesDetailReportByConcernID(this IBaseRepository<SOrder> salesOrderRepository,
            IBaseRepository<SOrderDetail> SorderDetailRepository, IBaseRepository<Product> productRepository,
            IBaseRepository<Size> sizeRepository, IBaseRepository<ProductUnitType> productUnitTypeRepository,
            IBaseRepository<StockDetail> stockdetailRepository, IBaseRepository<Category> categoryRepository, DateTime fromDate, DateTime toDate, int concernID)
        {
            var oSalesDetailData = (from SO in salesOrderRepository.All
                                    join SOD in SorderDetailRepository.All on SO.SOrderID equals SOD.SOrderID
                                    join STD in stockdetailRepository.All on SOD.SDetailID equals STD.SDetailID
                                    join P in productRepository.All on SOD.ProductID equals P.ProductID
                                    join CAT in categoryRepository.All on P.CategoryID equals CAT.CategoryID
                                    join PU in productUnitTypeRepository.All on (int)P.ProUnitTypeID equals PU.ProUnitTypeID
                                    join SZ in sizeRepository.All on P.SizeID equals SZ.SizeID
                                    join std in stockdetailRepository.All on SOD.SDetailID equals std.SDetailID
                                    where (SO.InvoiceDate >= fromDate && SO.InvoiceDate <= toDate && SO.Status == (int)EnumSalesType.Sales && SO.IsReplacement != 1)
                                    select new ProductWiseSalesReportModel
                                    {
                                        SOrderID = SO.SOrderID,
                                        InvoiceNo = SO.InvoiceNo,
                                        CustomerName = SO.Customer.Name,
                                        Date = SO.InvoiceDate,
                                        GrandTotal = SO.GrandTotal,
                                        NetDiscount = SO.NetDiscount,
                                        TotalAmount = SO.TotalAmount,
                                        RecAmount = (decimal)SO.RecAmount,
                                        PaymentDue = SO.PaymentDue,
                                        ProductID = P.ProductID,
                                        ProductName = P.ProductName,
                                        UnitPrice = SOD.SFTRate == 0 ? SOD.UnitPrice / PU.ConvertValue : SOD.SFTRate,
                                        UTAmount = SOD.UTAmount,
                                        PPDAmount = SOD.PPDAmount,
                                        PPOffer = SOD.PPOffer,
                                        Quantity = SOD.Quantity,
                                        IMEI = std.IMENO,
                                        ColorName = std.Color.Name,
                                        AdjAmount = SO.AdjAmount,
                                        UnitName = PU.Description,
                                        ProductCode = P.Code,
                                        IDCode = P.IDCode,
                                        SizeName = SZ.Description,
                                        CategoryName = CAT.Description,
                                        ConvertValue = P.BundleQty != 0 ? P.BundleQty : PU.ConvertValue,
                                        SalesPerCartonSft = P.SalesCSft,
                                        ExtraAmt = SOD.FractionAmt,
                                        ExtraSFT = SOD.TotalSFT - SOD.ActualSFT,
                                        PurchaseRate = STD.PRate,
                                        PurchaseSFTRate = STD.SFTRate






                                    }).OrderBy(x => x.SOrderID).ToList();

            var Replacements = (from SO in salesOrderRepository.All
                                join SOD in SorderDetailRepository.All on SO.SOrderID equals SOD.RepOrderID
                                join P in productRepository.All on SOD.ProductID equals P.ProductID
                                join PU in productUnitTypeRepository.All on (int)P.ProUnitTypeID equals PU.ProUnitTypeID
                                join SZ in sizeRepository.All on P.SizeID equals SZ.SizeID
                                join std in stockdetailRepository.All on SOD.RStockDetailID equals std.SDetailID
                                where (SO.InvoiceDate >= fromDate && SO.InvoiceDate <= toDate && SO.Status == (int)EnumSalesType.Sales && SO.IsReplacement == 1)
                                select new ProductWiseSalesReportModel
                                {
                                    SOrderID = SO.SOrderID,
                                    InvoiceNo = SO.InvoiceNo,
                                    CustomerName = SO.Customer.Name,
                                    Date = SO.InvoiceDate,
                                    GrandTotal = SO.GrandTotal,
                                    NetDiscount = SO.NetDiscount,
                                    TotalAmount = SO.TotalAmount,
                                    RecAmount = (decimal)SO.RecAmount,
                                    PaymentDue = SO.PaymentDue,
                                    ProductID = P.ProductID,
                                    ProductName = P.ProductName,
                                    UnitPrice = SOD.UnitPrice,
                                    UTAmount = SOD.UTAmount,
                                    PPDAmount = SOD.PPDAmount,
                                    PPOffer = SOD.PPOffer,
                                    Quantity = SOD.Quantity,
                                    IMEI = std.IMENO,
                                    ColorName = std.Color.Name,
                                    AdjAmount = SO.AdjAmount,
                                    UnitName = PU.Description,
                                    ProductCode = string.IsNullOrEmpty(P.IDCode) ? P.Code : P.IDCode,
                                    SizeName = SZ.Code,
                                    ConvertValue = P.BundleQty != 0 ? P.BundleQty : PU.ConvertValue,
                                    SalesPerCartonSft = P.SalesCSft,

                                }).OrderBy(x => x.SOrderID).ToList();

            oSalesDetailData.AddRange(Replacements);

            return oSalesDetailData;
        }

        public static IEnumerable<ProductWiseSalesReportModel> GetSalesDetailReportAdminByConcernID(this IBaseRepository<SOrder> salesOrderRepository,
           IBaseRepository<SOrderDetail> SorderDetailRepository, IBaseRepository<Product> productRepository,
           IBaseRepository<Size> sizeRepository, IBaseRepository<ProductUnitType> productUnitTypeRepository,
           IBaseRepository<StockDetail> stockdetailRepository, IBaseRepository<Category> categoryRepository, IBaseRepository<SisterConcern> sisterConcernRepository, IBaseRepository<Customer> customerRepository, DateTime fromDate, DateTime toDate, int concernID, int CustomerType)
        {
            var SisterConcerns = sisterConcernRepository.GetAll();
            if (concernID != 0)
                SisterConcerns = SisterConcerns.Where(o => o.ConcernID == concernID);
            var Customers = customerRepository.GetAll();
            if (CustomerType == 1)
                Customers = Customers.Where(o => o.CustomerType == EnumCustomerType.Retail);
            else if (CustomerType == 2)
                Customers = Customers.Where(o => o.CustomerType == EnumCustomerType.Dealer);

            var oSalesDetailData = (from SO in salesOrderRepository.GetAll()

                                    join SIS in SisterConcerns on SO.ConcernID equals SIS.ConcernID
                                    join CUS in Customers on SO.CustomerID equals CUS.CustomerID
                                    join SOD in SorderDetailRepository.GetAll() on SO.SOrderID equals SOD.SOrderID
                                    join P in productRepository.GetAll() on SOD.ProductID equals P.ProductID
                                    join CAT in categoryRepository.GetAll() on P.CategoryID equals CAT.CategoryID
                                    join PU in productUnitTypeRepository.GetAll() on (int)P.ProUnitTypeID equals PU.ProUnitTypeID
                                    join SZ in sizeRepository.GetAll() on P.SizeID equals SZ.SizeID
                                    join std in stockdetailRepository.GetAll() on SOD.SDetailID equals std.SDetailID

                                    where (SO.InvoiceDate >= fromDate && SO.InvoiceDate <= toDate && SO.Status == (int)EnumSalesType.Sales && SO.IsReplacement != 1)
                                    select new ProductWiseSalesReportModel
                                    {
                                        SOrderID = SO.SOrderID,
                                        InvoiceNo = SO.InvoiceNo,
                                        CustomerName = SO.Customer.Name,
                                        Date = SO.InvoiceDate,
                                        GrandTotal = SO.GrandTotal,
                                        NetDiscount = SO.NetDiscount,
                                        TotalAmount = SO.TotalAmount,
                                        RecAmount = (decimal)SO.RecAmount,
                                        PaymentDue = SO.PaymentDue,
                                        ProductID = P.ProductID,
                                        ProductName = P.ProductName,
                                        UnitPrice = SOD.SFTRate == 0 ? SOD.UnitPrice / PU.ConvertValue : SOD.SFTRate,
                                        UTAmount = SOD.UTAmount,
                                        PPDAmount = SOD.PPDAmount,
                                        PPOffer = SOD.PPOffer,
                                        Quantity = SOD.Quantity,
                                        IMEI = std.IMENO,
                                        ColorName = std.Color.Name,
                                        AdjAmount = SO.AdjAmount,
                                        UnitName = PU.Description,
                                        ProductCode = P.Code,
                                        IDCode = P.IDCode,
                                        SizeName = SZ.Description,
                                        CategoryName = CAT.Description,
                                        ConvertValue = P.BundleQty != 0 ? P.BundleQty : PU.ConvertValue,
                                        SalesPerCartonSft = P.SalesCSft,
                                        ConcernName = SIS.Name
                                    }).OrderBy(x => x.SOrderID).ToList();

            var Replacements = (from SO in salesOrderRepository.GetAll()
                                join SIS in SisterConcerns on SO.ConcernID equals SIS.ConcernID
                                join SOD in SorderDetailRepository.GetAll() on SO.SOrderID equals SOD.RepOrderID
                                join P in productRepository.GetAll() on SOD.ProductID equals P.ProductID
                                join PU in productUnitTypeRepository.GetAll() on (int)P.ProUnitTypeID equals PU.ProUnitTypeID
                                join SZ in sizeRepository.GetAll() on P.SizeID equals SZ.SizeID
                                join std in stockdetailRepository.GetAll() on SOD.RStockDetailID equals std.SDetailID
                                where (SO.InvoiceDate >= fromDate && SO.InvoiceDate <= toDate && SO.Status == (int)EnumSalesType.Sales && SO.IsReplacement == 1)
                                select new ProductWiseSalesReportModel
                                {
                                    SOrderID = SO.SOrderID,
                                    InvoiceNo = SO.InvoiceNo,
                                    CustomerName = SO.Customer.Name,
                                    Date = SO.InvoiceDate,
                                    GrandTotal = SO.GrandTotal,
                                    NetDiscount = SO.NetDiscount,
                                    TotalAmount = SO.TotalAmount,
                                    RecAmount = (decimal)SO.RecAmount,
                                    PaymentDue = SO.PaymentDue,
                                    ProductID = P.ProductID,
                                    ProductName = P.ProductName,
                                    UnitPrice = SOD.UnitPrice,
                                    UTAmount = SOD.UTAmount,
                                    PPDAmount = SOD.PPDAmount,
                                    PPOffer = SOD.PPOffer,
                                    Quantity = SOD.Quantity,
                                    IMEI = std.IMENO,
                                    ColorName = std.Color.Name,
                                    AdjAmount = SO.AdjAmount,
                                    UnitName = PU.Description,
                                    ProductCode = string.IsNullOrEmpty(P.IDCode) ? P.Code : P.IDCode,
                                    SizeName = SZ.Code,
                                    ConvertValue = P.BundleQty != 0 ? P.BundleQty : PU.ConvertValue,
                                    SalesPerCartonSft = P.SalesCSft,
                                    ConcernName = SIS.Name
                                }).OrderBy(x => x.SOrderID).ToList();

            oSalesDetailData.AddRange(Replacements);

            return oSalesDetailData;
        }
        public static IEnumerable<SOredersReportModel> GetSalesDetailReportByCustomerID(
                                                            this IBaseRepository<SOrder> salesOrderRepository, IBaseRepository<SOrderDetail> SorderDetailRepository,
                                                            IBaseRepository<Product> productRepository, IBaseRepository<StockDetail> stockdetailRepository,
                                                            IBaseRepository<Color> ColorRepository,
                                                            DateTime fromDate, DateTime toDate, int customerID
            )
        {
            var oSalesDetailData = (from SO in salesOrderRepository.All
                                    join SOD in SorderDetailRepository.All on SO.SOrderID equals SOD.SOrderID
                                    join P in productRepository.All on SOD.ProductID equals P.ProductID
                                    join std in stockdetailRepository.All on SOD.SDetailID equals std.SDetailID
                                    join col in ColorRepository.All on std.ColorID equals col.ColorID
                                    where (SO.InvoiceDate >= fromDate && SO.InvoiceDate <= toDate && SO.Status == (int)EnumSalesType.Sales && SO.CustomerID == customerID && SO.IsReplacement != 1)
                                    select new SOredersReportModel
                                    {
                                        CustomerID = SO.CustomerID,
                                        CustomerName = SO.Customer.Name,
                                        CustomerCode = SO.Customer.Code,
                                        CustomerAddress = SO.Customer.Address,
                                        CustomerContactNo = SO.Customer.ContactNo,
                                        CustCompanyName = SO.Customer.CompanyName,
                                        CustomerTotalDue = SO.Customer.TotalDue,
                                        SOrderID = SO.SOrderID,
                                        InvoiceNo = SO.InvoiceNo,
                                        InvoiceDate = SO.InvoiceDate,
                                        Grandtotal = SO.GrandTotal,
                                        FlatDiscount = SO.TDAmount,
                                        TotalAmount = SO.TotalAmount,
                                        NetDiscount = SO.NetDiscount,
                                        RecAmount = (decimal)SO.RecAmount,
                                        PaymentDue = SO.PaymentDue,
                                        AdjAmount = SO.AdjAmount,
                                        ProductID = P.ProductID,
                                        ProductName = P.ProductName,
                                        UnitPrice = SOD.UnitPrice - SOD.PPDAmount,
                                        UTAmount = SOD.UTAmount,
                                        PPDAmount = SOD.PPDAmount,
                                        PPDPercentage = SOD.PPDPercentage,
                                        Quantity = SOD.Quantity,
                                        IMENO = std.IMENO,
                                        ColorName = col.Name,
                                        CustomerType = SO.Customer.CustomerType,
                                        CustomerNID = SO.Customer.NID
                                    }).OrderByDescending(x => x.SOrderID).ToList();
            var ReplacementOrders = (from SO in salesOrderRepository.All
                                     join SOD in SorderDetailRepository.All on SO.SOrderID equals SOD.RepOrderID
                                     join P in productRepository.All on SOD.ProductID equals P.ProductID
                                     join std in stockdetailRepository.All on SOD.RStockDetailID equals std.SDetailID
                                     join col in ColorRepository.All on std.ColorID equals col.ColorID
                                     where (SO.InvoiceDate >= fromDate && SO.InvoiceDate <= toDate && SO.Status == (int)EnumSalesType.Sales && SO.CustomerID == customerID && SO.IsReplacement == 1)
                                     select new SOredersReportModel
                                     {
                                         CustomerID = SO.CustomerID,
                                         CustomerName = SO.Customer.Name,
                                         CustomerCode = SO.Customer.Code,
                                         CustomerAddress = SO.Customer.Address,
                                         CustomerContactNo = SO.Customer.ContactNo,
                                         CustCompanyName = SO.Customer.CompanyName,
                                         CustomerTotalDue = SO.Customer.TotalDue,
                                         SOrderID = SO.SOrderID,
                                         InvoiceNo = SO.InvoiceNo,
                                         InvoiceDate = SO.InvoiceDate,
                                         Grandtotal = SO.GrandTotal,
                                         FlatDiscount = SO.TDAmount,
                                         TotalAmount = SO.TotalAmount,
                                         NetDiscount = SO.NetDiscount,
                                         RecAmount = (decimal)SO.RecAmount,
                                         PaymentDue = SO.PaymentDue,
                                         AdjAmount = SO.AdjAmount,
                                         ProductID = P.ProductID,
                                         ProductName = P.ProductName,
                                         UnitPrice = SOD.UnitPrice - SOD.PPDAmount,
                                         UTAmount = SOD.UTAmount,
                                         PPDAmount = SOD.PPDAmount,
                                         PPDPercentage = SOD.PPDPercentage,
                                         Quantity = SOD.Quantity,
                                         IMENO = std.IMENO,
                                         ColorName = col.Name,
                                         CustomerType = SO.Customer.CustomerType,
                                         CustomerNID = SO.Customer.NID
                                     }).OrderByDescending(x => x.SOrderID).ToList();

            oSalesDetailData.AddRange(ReplacementOrders);

            return oSalesDetailData;
        }

        public static IEnumerable<Tuple<string, DateTime, string, string, decimal, decimal, Tuple<decimal, decimal, decimal, decimal>>>
        GetSalesDetailReportByMOID(this IBaseRepository<SOrder> salesOrderRepository, IBaseRepository<Customer> customerRepository, IBaseRepository<Employee> employeeRepository,
        DateTime fromDate, DateTime toDate, int concernID, int MOID, int RptType)
        {
            //var oMOWiseSalesDetailData = (dynamic)null;

            if (RptType == 1)
            {
                var oAllMOWiseSalesDetailData = (from CO in customerRepository.All
                                                 from SO in salesOrderRepository.All
                                                 from Emp in employeeRepository.All
                                                 where (CO.CustomerID == SO.CustomerID && SO.Status == 1 && CO.EmployeeID == Emp.EmployeeID && (SO.InvoiceDate >= fromDate && SO.InvoiceDate <= toDate))
                                                 select new
                                                 {
                                                     Emp.Name,
                                                     SO.InvoiceDate,
                                                     CusName = CO.Name,
                                                     SO.InvoiceNo,
                                                     SO.GrandTotal,
                                                     SO.NetDiscount,
                                                     SO.TotalAmount,
                                                     SO.RecAmount,
                                                     SO.PaymentDue,
                                                     SO.AdjAmount
                                                 }).OrderByDescending(x => x.InvoiceDate).ToList();



                return oAllMOWiseSalesDetailData.Select(x => new Tuple<string, DateTime, string, string, decimal, decimal, Tuple<decimal, decimal, decimal, decimal>>
                    (
                     x.Name,
                     x.InvoiceDate,
                     x.CusName,
                     x.InvoiceNo,
                     x.GrandTotal,
                     x.NetDiscount, new Tuple<decimal, decimal, decimal, decimal>(
                                        x.TotalAmount,
                                       (decimal)x.RecAmount,
                                       x.PaymentDue,
                                       x.AdjAmount
                                       )
                    ));
            }
            else
            {
                var oMOWiseSalesDetailData = (from CO in customerRepository.All
                                              from SO in salesOrderRepository.All
                                              from Emp in employeeRepository.All
                                              where (CO.CustomerID == SO.CustomerID && SO.Status == 1 && CO.EmployeeID == Emp.EmployeeID && Emp.EmployeeID == MOID && (SO.InvoiceDate >= fromDate && SO.InvoiceDate <= toDate))
                                              select new { Emp.Name, SO.InvoiceDate, CusName = CO.Name, SO.InvoiceNo, SO.GrandTotal, SO.NetDiscount, SO.TotalAmount, SO.RecAmount, SO.PaymentDue, SO.AdjAmount }).OrderByDescending(x => x.InvoiceDate).ToList();



                return oMOWiseSalesDetailData.Select(x => new Tuple<string, DateTime, string, string, decimal, decimal, Tuple<decimal, decimal, decimal, decimal>>
                    (
                     x.Name,
                     x.InvoiceDate,
                     x.CusName,
                     x.InvoiceNo,
                     x.GrandTotal,
                     x.NetDiscount, new Tuple<decimal, decimal, decimal, decimal>(
                                        x.TotalAmount,
                                       (decimal)x.RecAmount,
                                       x.PaymentDue,
                                       x.AdjAmount)
                    ));
            }


        }

        public static IEnumerable<Tuple<string, string, string, string, string, decimal>>
        GetMOWiseCustomerDueRpt(this IBaseRepository<SOrder> salesOrderRepository, IBaseRepository<Customer> customerRepository, IBaseRepository<Employee> employeeRepository,
        int concernID, int MOID, int RptType)
        {
            if (RptType == 1)
            {
                var oAllMOWiseCustomerDue = (from CO in customerRepository.All
                                             from Emp in employeeRepository.All
                                             where (CO.EmployeeID == Emp.EmployeeID && CO.TotalDue != 0)
                                             select new { EmpName = Emp.Name, CusCode = CO.Code, CusName = CO.Name, CusContact = CO.ContactNo, Address = CO.Address, TotalDue = CO.TotalDue }).OrderBy(x => x.CusCode).ToList();



                return oAllMOWiseCustomerDue.Select(x => new Tuple<string, string, string, string, string, decimal>
                    (
                     x.EmpName,
                     x.CusCode,
                     x.CusName,
                     x.CusContact,
                     x.Address,
                     x.TotalDue
                    ));
            }
            else
            {
                var oAllMOWiseCustomerDue = (from CO in customerRepository.All
                                             from Emp in employeeRepository.All
                                             where (CO.EmployeeID == Emp.EmployeeID && Emp.EmployeeID == MOID && CO.TotalDue != 0)
                                             select new { EmpName = Emp.Name, CusCode = CO.Code, CusName = CO.Name, CusContact = CO.ContactNo, Address = CO.Address, TotalDue = CO.TotalDue }).OrderBy(x => x.CusCode).ToList();



                return oAllMOWiseCustomerDue.Select(x => new Tuple<string, string, string, string, string, decimal>
                    (
                     x.EmpName,
                     x.CusCode,
                     x.CusName,
                     x.CusContact,
                     x.Address,
                     x.TotalDue
                    ));
            }


        }


        public static IEnumerable<Tuple<int, int, int, int, string, string, string,
            Tuple<decimal, decimal, decimal, decimal, decimal, decimal, int, Tuple<string, decimal, int, int, string, string, string, Tuple<int, string, int>>>>>
            GetSalesOrderDetailByOrderId(this IBaseRepository<SOrderDetail> salesOrderDetailRepository, int orderId, IBaseRepository<Product> productRepository,
            IBaseRepository<Color> colorRepository, IBaseRepository<StockDetail> stockDetailRepository, IBaseRepository<ProductUnitType> ProductUnitTypeRepo, IBaseRepository<Category> categoryRepository, IBaseRepository<Size> sizeRepository)
        {
            IQueryable<Product> products = productRepository.All;
            IQueryable<Color> colors = colorRepository.All;
            IQueryable<StockDetail> details = stockDetailRepository.All;

            var items = (from sod in salesOrderDetailRepository.All
                         join p in productRepository.All on sod.ProductID equals p.ProductID
                         join pu in ProductUnitTypeRepo.All on p.ProUnitTypeID equals pu.ProUnitTypeID
                         join sd in stockDetailRepository.All on sod.SDetailID equals sd.SDetailID
                         join c in colorRepository.All on sd.ColorID equals c.ColorID
                         join ca in categoryRepository.All on p.CategoryID equals ca.CategoryID
                         join s in sizeRepository.All on p.SizeID equals s.SizeID
                         where sod.SOrderID == orderId
                         select new
                         {
                             sod.SOrderDetailID,
                             sod.SOrderID,
                             sod.ProductID,
                             StockDetailID = sod.SDetailID,
                             ProductName = p.ProductName,
                             ProductType = p.ProductType,
                             ProductUnitType = p.ProUnitTypeID,
                             p.Code,
                             sd.IMENO,
                             sod.Quantity,
                             sod.UnitPrice,
                             sod.MPRate,
                             sod.UTAmount,
                             sod.PPDPercentage,
                             sod.PPDAmount,
                             UnitType = pu.Description,
                             CategoryName = ca.Description,
                             SizeName = s.Description,
                             ColorId = sd.ColorID,
                             ColorName = c.Name,
                             ConvertValue = p.BundleQty == 0 ? pu.ConvertValue : p.BundleQty,
                             VehicleID = sod.VehicleID,
                             VehicleNo = sod.VehicleNo,
                             OrderIndex = sod.OrderIndex
                         }
            ).ToList();

            return items.Select(x => new Tuple<int, int, int, int, string, string, string,
                Tuple<decimal, decimal, decimal, decimal, decimal, decimal, int, Tuple<string, decimal, int, int, string, string, string, Tuple<int, string, int>>>>
                (
                    x.SOrderDetailID,
                    x.SOrderID,
                    x.ProductID,
                    x.StockDetailID,
                    x.ProductName,
                    x.Code,
                    x.IMENO,
                    new Tuple<decimal, decimal, decimal, decimal, decimal, decimal, int, Tuple<string, decimal, int, int, string, string, string, Tuple<int, string, int>>>(
                    x.Quantity,
                    x.UnitPrice,
                    x.MPRate,
                    x.UTAmount,
                    x.PPDPercentage,
                    x.PPDAmount,
                    x.ColorId,
                    new Tuple<string, decimal, int, int, string, string, string, Tuple<int, string, int>>(
                        x.ColorName,
                        x.ConvertValue,
                        x.ProductType,
                        x.ProductUnitType,
                        x.UnitType,
                        x.CategoryName,
                        x.SizeName,
                        new Tuple<int, string, int>(x.ProductID, x.ProductName, x.OrderIndex))
                    )));

        }

        public static IEnumerable<Tuple<int, int, int, int, string, string, string,
        Tuple<decimal, decimal, decimal, decimal, decimal, decimal, int, Tuple<string, decimal, string, string, string, string>>>>
        GetSalesOrderDetailByOrderIdForInvoice(this IBaseRepository<SOrderDetail> salesOrderDetailRepository, int orderId, IBaseRepository<Product> productRepository,
        IBaseRepository<Color> colorRepository, IBaseRepository<StockDetail> stockDetailRepository)
        {
            IQueryable<Product> products = productRepository.All;
            IQueryable<Color> colors = colorRepository.All;
            IQueryable<StockDetail> details = stockDetailRepository.All;

            var items = salesOrderDetailRepository.All.
                GroupJoin(products, s => s.ProductID, p => p.ProductID,
                (s, p) => new { SalesOrder = s, Products = p }).
                SelectMany(x => x.Products.DefaultIfEmpty(), (s, p) => new { SalesOrder = s.SalesOrder, Products = p }).
                GroupJoin(details, s => s.SalesOrder.SDetailID, d => d.SDetailID,
                (s, d) => new { SalesOrder = s.SalesOrder, Products = s.Products, Details = d }).
                SelectMany(x => x.Details.DefaultIfEmpty(), (s, d) => new { SalesOrder = s.SalesOrder, Products = s.Products, Details = d }).
                GroupJoin(colors, s => s.Details.ColorID, c => c.ColorID,
                (d, c) => new { SalesOrder = d.SalesOrder, Details = d.Details, Products = d.Products, Colors = c }).
                SelectMany(x => x.Colors.DefaultIfEmpty(), (d, c) => new { SalesOrder = d.SalesOrder, Products = d.Products, Details = d.Details, Color = c }).
                Where(x => x.SalesOrder.SOrderID == orderId).
                Select(x => new
                {
                    x.SalesOrder.SOrderDetailID,
                    x.SalesOrder.SOrderID,
                    x.SalesOrder.ProductID,
                    StockDetailID = x.SalesOrder.SDetailID,
                    x.Products.ProductName,
                    x.Products.Code,
                    x.Details.IMENO,
                    x.SalesOrder.Quantity,
                    x.SalesOrder.UnitPrice,
                    x.SalesOrder.MPRate,
                    x.SalesOrder.UTAmount,
                    x.SalesOrder.PPDPercentage,
                    x.SalesOrder.PPDAmount,
                    ColorId = x.Color.ColorID,
                    ColorName = x.Color.Name,
                    PPOffer = x.SalesOrder.PPOffer,
                    x.Products.CompressorWarrentyMonth,
                    x.Products.MotorWarrentyMonth,
                    x.Products.PanelWarrentyMonth,
                    x.Products.SparePartsWarrentyMonth
                }).ToList();

            return items.Select(x => new Tuple<int, int, int, int, string, string, string,
                Tuple<decimal, decimal, decimal, decimal, decimal, decimal, int, Tuple<string, decimal, string, string, string, string>>>
                (
                    x.SOrderDetailID,
                    x.SOrderID,
                    x.ProductID,
                    x.StockDetailID,
                    x.ProductName,
                    x.Code,
                    x.IMENO,
                    new Tuple<decimal, decimal, decimal, decimal, decimal, decimal, int, Tuple<string, decimal, string, string, string, string>>(
                    x.Quantity,
                    x.UnitPrice,
                    x.MPRate,
                    x.UTAmount,
                    x.PPDPercentage,
                    x.PPDAmount,
                    x.ColorId,
                    new Tuple<string, decimal, string, string, string, string>(
                        x.ColorName,
                        x.PPOffer,
                        x.CompressorWarrentyMonth,
                        x.MotorWarrentyMonth,
                        x.PanelWarrentyMonth,
                        x.SparePartsWarrentyMonth
                        ))
                    ));
        }

        public static IEnumerable<Tuple<DateTime, string, string, decimal, decimal>>
            GetSalesByProductID(this IBaseRepository<SOrder> salesOrderRepository, IBaseRepository<SOrderDetail> SorderDetailRepository, IBaseRepository<Product> productRepository,
            DateTime fromDate, DateTime toDate, int productID)
        {
            var oSalesDetailData = (from SOD in SorderDetailRepository.All
                                    from SO in salesOrderRepository.All
                                    from P in productRepository.All
                                    where (SOD.SOrderID == SO.SOrderID && P.ProductID == SOD.ProductID && SO.InvoiceDate >= fromDate && SO.InvoiceDate <= toDate && SO.Status == 1 && SOD.ProductID == productID)
                                    select new { SO.InvoiceNo, SO.InvoiceDate, SO.GrandTotal, SO.TDAmount, SO.TotalAmount, SO.RecAmount, SO.PaymentDue, P.ProductID, P.ProductName, SOD.UnitPrice, SOD.UTAmount, SOD.PPDAmount, SOD.Quantity }).OrderByDescending(x => x.InvoiceDate).ToList();

            return oSalesDetailData.Select(x => new Tuple<DateTime, string, string, decimal, decimal>
                (
                 x.InvoiceDate,
                 x.InvoiceNo,
                x.ProductName,
                x.UnitPrice,
                x.Quantity
                ));
        }

        public static IEnumerable<Tuple<DateTime, string, string, decimal, decimal>> GetSalesByProductID(this IBaseRepository<SOrder> SOrderRepository, IBaseRepository<SOrderDetail> SOrderDetailRepository, IBaseRepository<Product> productRepository,
        IBaseRepository<CreditSale> CreditSalesRepository, IBaseRepository<CreditSaleDetails> CreditSalesDetailRepository, DateTime fromDate, DateTime toDate, int ConcernID, int productid)
        {
            //var oSales = ((from POD in SOrderDetailRepository.All
            //               from PO in SOrderRepository.All
            //               from P in productRepository.All
            //               where (POD.SOrderID == PO.SOrderID && P.ProductID == POD.ProductID && PO.InvoiceDate >= fromDate && PO.InvoiceDate <= toDate && P.ProductID == productid && PO.Status == 1)
            //               select new {POD.StockDetailID, PO.InvoiceNo, SalesDate = PO.InvoiceDate, P.ProductName, POD.Quantity, POD.UnitPrice })
            //                 .Union(
            //                              from SOD in CreditSalesDetailRepository.All
            //                              from SO in CreditSalesRepository.All
            //                              from P in productRepository.All
            //                              where SOD.CreditSalesID == SO.CreditSalesID && P.ProductID == SOD.ProductID
            //                              && P.ProductID == productid && SO.SalesDate >= fromDate && SO.SalesDate <= toDate
            //                              select new
            //                              {
            //                                  SOD.StockDetailID,
            //                                  InvoiceNo = SO.InvoiceNo + " (Credit)",
            //                                  SO.SalesDate,
            //                                  P.ProductName,
            //                                  SOD.Quantity,
            //                                  SOD.UnitPrice
            //                              })).OrderBy(x => x.SalesDate).ToList();

            //return oSales.Select(x => new Tuple<DateTime, string, string, decimal, decimal>
            //    (
            //     x.SalesDate,
            //     x.InvoiceNo,
            //    x.ProductName,
            //    x.Quantity,
            //    x.UnitPrice
            //    ));


            var oSales = ((from POD in SOrderDetailRepository.All
                           from PO in SOrderRepository.All
                           from P in productRepository.All
                           where (POD.SOrderID == PO.SOrderID && P.ProductID == POD.ProductID && PO.InvoiceDate >= fromDate && PO.InvoiceDate <= toDate && P.ProductID == productid && PO.Status == 1)
                           group POD by new { PO.InvoiceNo, PO.InvoiceDate, P.ProductName, POD.UnitPrice } into g
                           select new { g.Key.InvoiceNo, SalesDate = g.Key.InvoiceDate, g.Key.ProductName, Quantity = g.Sum(x => x.Quantity), g.Key.UnitPrice })
                                         .Union(
                                                      from SOD in CreditSalesDetailRepository.All
                                                      from SO in CreditSalesRepository.All
                                                      from P in productRepository.All
                                                      where SOD.CreditSalesID == SO.CreditSalesID && P.ProductID == SOD.ProductID
                                                      && P.ProductID == productid && SO.SalesDate >= fromDate && SO.SalesDate <= toDate
                                                      group SOD by new { SO.InvoiceNo, SO.SalesDate, P.ProductName, SOD.UnitPrice } into g
                                                      select new
                                                      {
                                                          InvoiceNo = g.Key.InvoiceNo + " (Credit)",
                                                          g.Key.SalesDate,
                                                          g.Key.ProductName,
                                                          Quantity = g.Sum(x => x.Quantity),
                                                          g.Key.UnitPrice
                                                      })).OrderBy(x => x.SalesDate).ToList();

            return oSales.Select(x => new Tuple<DateTime, string, string, decimal, decimal>
                (
                 x.SalesDate,
                 x.InvoiceNo,
                x.ProductName,
                x.Quantity,
                x.UnitPrice
                ));
        }

        /// <summary>
        /// Author:Aminul
        /// Date: 06/03/2018
        /// </summary>
        /// <param name="SOrderRepository"></param>
        /// <param name="SOrderDetailRepo"></param>
        /// <param name="CustomerRepo"></param>
        /// <param name="CustomerID"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Tuple<int, string, DateTime, string,
            string, decimal, EnumSalesType>>> GetReplacementOrdersAsync(this IBaseRepository<SOrder> SOrderRepository, IBaseRepository<SOrderDetail> SOrderDetailRepo, IBaseRepository<Customer> CustomerRepo, int EmployeeID)
        {
            var SOrders = SOrderRepository.All;
            //var SOrderDetails = SOrderDetailRepo.All;
            IQueryable<Customer> CustomerList = null;
            if (EmployeeID != 0)
                CustomerList = CustomerRepo.All.Where(i => i.EmployeeID == EmployeeID);
            else
                CustomerList = CustomerRepo.All;
            var result = await (from so in SOrders.Where(i => i.IsReplacement == 1)
                                join cus in CustomerList on so.CustomerID equals cus.CustomerID
                                select new
                                {
                                    so.SOrderID,
                                    so.InvoiceNo,
                                    SalesDate = so.InvoiceDate,
                                    CustomerName = cus.Name,
                                    cus.ContactNo,
                                    cus.TotalDue,
                                    so.Status
                                }).OrderByDescending(s => s.SOrderID).ToListAsync();


            return result.Select(x => new Tuple<int, string, DateTime, string, string, decimal, EnumSalesType>
                (
                    x.SOrderID,
                    x.InvoiceNo,
                    x.SalesDate,
                    x.CustomerName,
                    x.ContactNo,
                    x.TotalDue,
                    (EnumSalesType)x.Status
                )).ToList();
        }

        public static async Task<IEnumerable<Tuple<int, string, DateTime, string,
    string, decimal, EnumSalesType>>> GetReturnOrdersAsync(this IBaseRepository<SOrder> SOrderRepository, IBaseRepository<SOrderDetail> SOrderDetailRepo, IBaseRepository<Customer> CustomerRepo)
        {
            var SOrders = SOrderRepository.All;
            //var SOrderDetails = SOrderDetailRepo.All;
            var Customers = CustomerRepo.All;

            var result = await (from so in SOrders
                                where so.Status == (int)EnumSalesType.ProductReturn
                                join cus in Customers on so.CustomerID equals cus.CustomerID
                                select new
                                {
                                    so.SOrderID,
                                    so.InvoiceNo,
                                    SalesDate = so.InvoiceDate,
                                    CustomerName = cus.Name,
                                    cus.ContactNo,
                                    cus.TotalDue,
                                    so.Status
                                }).OrderByDescending(s => s.SOrderID).ToListAsync();


            return result.Select(x => new Tuple<int, string, DateTime, string, string, decimal, EnumSalesType>
                (
                    x.SOrderID,
                    x.InvoiceNo,
                    x.SalesDate,
                    x.CustomerName,
                    x.ContactNo,
                    x.TotalDue,
                    (EnumSalesType)x.Status
                )).ToList();
        }

        public static List<ReplaceOrderDetail> GetReplaceOrderInvoiceReportByID(this IBaseRepository<SOrder> SOrderRepository, IBaseRepository<SOrderDetail> SOrderDetailRepo, IBaseRepository<StockDetail> StockDetailRepo, IBaseRepository<Product> ProductRepo, int OrderID)
        {
            List<ReplaceOrderDetail> list = new List<ReplaceOrderDetail>();
            var dbsorder = SOrderRepository.FindBy(i => i.SOrderID == OrderID);
            var sorderDetails = SOrderDetailRepo.All;
            var stockdetails = StockDetailRepo.All;
            var products = ProductRepo.All;

            var dresult = (from so in dbsorder
                           join sod in sorderDetails on so.SOrderID equals sod.RepOrderID
                           join std in stockdetails on sod.SDetailID equals std.SDetailID
                           join p in products on std.ProductID equals p.ProductID
                           select new
                           {
                               SOrderDetailID = sod.SOrderDetailID,
                               DamageIMEINO = std.IMENO,
                               DamageProductName = p.ProductName,
                               DamageUnitPrice = sod.UnitPrice.ToString(),
                               Quantity = 1,
                               Remarks = sod.Remarks
                           }).ToList();

            var rresult = (from so in dbsorder
                           join sod in sorderDetails on so.SOrderID equals sod.RepOrderID
                           join std in stockdetails on sod.RStockDetailID equals std.SDetailID
                           join p in products on std.ProductID equals p.ProductID
                           select new
                           {
                               SOrderDetailID = sod.SOrderDetailID,
                               ReplaceIMEINO = std.IMENO,
                               ProductName = p.ProductName,
                               UnitPrice = sod.RepUnitPrice,
                               Quantity = 1,
                               Remarks = sod.Remarks
                           }).ToList();

            var result = (from d in dresult
                          join r in rresult on d.SOrderDetailID equals r.SOrderDetailID
                          select new ReplaceOrderDetail
                          {
                              DamageProductName = d.DamageProductName,
                              DamageIMEINO = d.DamageIMEINO,
                              DamageUnitPrice = d.DamageUnitPrice,
                              Quantity = d.Quantity,
                              ProductName = r.ProductName,
                              ReplaceIMEINO = r.ReplaceIMEINO,
                              UnitPrice = (decimal)r.UnitPrice,
                              Remarks = r.Remarks
                          }).ToList();
            return result;

        }

        public static List<ReplaceOrderDetail> GetReturnOrderInvoiceReportByID(this IBaseRepository<SOrder> SOrderRepository, IBaseRepository<SOrderDetail> SOrderDetailRepo, IBaseRepository<StockDetail> StockDetailRepo, IBaseRepository<Product> ProductRepo, int OrderID)
        {
            List<ReplaceOrderDetail> list = new List<ReplaceOrderDetail>();
            var dbsorder = SOrderRepository.FindBy(i => i.SOrderID == OrderID);
            var sorderDetails = SOrderDetailRepo.All;
            var stockdetails = StockDetailRepo.All;
            var products = ProductRepo.All;

            var dresult = (from so in dbsorder
                           join sod in sorderDetails on so.SOrderID equals sod.SOrderID
                           join std in stockdetails on sod.SDetailID equals std.SDetailID
                           join p in products on std.ProductID equals p.ProductID
                           select new ReplaceOrderDetail
                           {
                               SOrderDetailID = sod.SOrderDetailID,
                               DamageIMEINO = std.IMENO,
                               DamageProductName = p.ProductName,
                               UnitPrice = sod.UnitPrice,
                               Quantity = 1,
                               MPRate = sod.MPRate
                           }).ToList();

            //var result = (from d in dresult
            //              select new ReplaceOrderDetail
            //              {
            //                  DamageProductName = d.DamageProductName,
            //                  DamageIMEINO = d.DamageIMEINO,
            //                  DamageUnitPrice = d.DamageUnitPrice,
            //                  Quantity = d.Quantity,
            //                  //ProductName = r.ProductName,
            //                  //ReplaceIMEINO = r.ReplaceIMEINO,
            //                  //UnitPrice = (decimal)r.UnitPrice
            //              }).ToList();
            return dresult;

        }


        public static List<ProductWiseSalesReportModel> ProductWiseSalesReport(this IBaseRepository<SOrder> SOrderRepository, IBaseRepository<SOrderDetail> SOrderDetailRepo, IBaseRepository<Customer> CustomerRepository, IBaseRepository<Employee> EmployeeRepository, IBaseRepository<Product> ProductRepository, int ConcernID, int CustomerID, DateTime fromDate, DateTime toDate)
        {
            List<SOrder> SOrders = new List<SOrder>();
            if (CustomerID != 0)
                SOrders = SOrderRepository.All.Where(i => i.CustomerID == CustomerID && i.InvoiceDate >= fromDate && i.InvoiceDate <= toDate && i.ConcernID == ConcernID).ToList();
            else
                SOrders = SOrderRepository.All.Where(i => i.InvoiceDate >= fromDate && i.InvoiceDate <= toDate && i.ConcernID == ConcernID).ToList();

            var SOrderDetails = SOrderDetailRepo.All;
            var Products = ProductRepository.All;
            var Customers = CustomerRepository.All;
            var Employees = EmployeeRepository.All;


            var result = from SO in SOrders.Where(i => i.Status == (int)EnumSalesType.Sales)
                         join SOD in SOrderDetails on SO.SOrderID equals SOD.SOrderID
                         join P in Products on SOD.ProductID equals P.ProductID
                         join C in Customers on SO.CustomerID equals C.CustomerID
                         join E in Employees on C.EmployeeID equals E.EmployeeID
                         select new ProductWiseSalesReportModel
                         {
                             Date = SO.InvoiceDate,
                             EmployeeCode = E.Code,
                             EmployeeName = E.Name,
                             CustomerCode = C.Code,
                             CustomerName = C.Name,
                             Address = C.Address,
                             Mobile = C.ContactNo,
                             ProductName = P.ProductName,
                             Quantity = SOD.Quantity,
                             SalesRate = SOD.UnitPrice - SOD.PPDAmount - SOD.PPOffer,
                             TotalAmount = SOD.UTAmount
                         };

            var fresult = from r in result
                          group r by new { r.Date, r.EmployeeCode, r.EmployeeName, r.CustomerCode, r.CustomerName, r.Address, r.Mobile, r.ProductName, r.SalesRate } into g
                          select new ProductWiseSalesReportModel
                          {
                              Date = g.Key.Date,
                              EmployeeCode = g.Key.EmployeeCode,
                              EmployeeName = g.Key.EmployeeName,
                              CustomerCode = g.Key.CustomerCode,
                              CustomerName = g.Key.CustomerName,
                              Address = g.Key.Address,
                              Mobile = g.Key.Mobile,
                              ProductName = g.Key.ProductName,
                              SalesRate = g.Key.SalesRate,
                              Quantity = g.Sum(i => i.Quantity),
                              TotalAmount = g.Sum(i => i.TotalAmount)
                          };

            return fresult.ToList();
        }

        public static List<ProductWiseSalesReportModel> ProductWiseSalesDetailsReport(this IBaseRepository<SOrder> SOrderRepository,
            IBaseRepository<SOrderDetail> SOrderDetailRepo, IBaseRepository<Company> CompanyRepository,
            IBaseRepository<Category> CategoryRepository, IBaseRepository<Product> ProductRepository, IBaseRepository<StockDetail> StockDetailRepository,
            int CompanyID, int CategoryID, int ProductID, DateTime fromDate, DateTime toDate)
        {
            var Products = ProductRepository.All;
            if (CompanyID != 0)
                Products = Products.Where(i => i.CompanyID == CompanyID);
            if (CategoryID != 0)
                Products = Products.Where(i => i.CategoryID == CategoryID);
            if (ProductID != 0)
                Products = Products.Where(i => i.ProductID == ProductID);

            var SOrderDetails = SOrderDetailRepo.All;
            var SOrders = SOrderRepository.All.Where(i => i.InvoiceDate >= fromDate && i.InvoiceDate <= toDate && i.Status == (int)EnumSalesType.Sales);

            var result = from SO in SOrders
                         join SOD in SOrderDetails on SO.SOrderID equals SOD.SOrderID
                         join STD in StockDetailRepository.All on SOD.SDetailID equals STD.SDetailID
                         join P in Products on SOD.ProductID equals P.ProductID
                         join COM in CompanyRepository.All on P.CompanyID equals COM.CompanyID
                         join CAT in CategoryRepository.All on P.CategoryID equals CAT.CategoryID
                         select new ProductWiseSalesReportModel
                         {
                             Date = SO.InvoiceDate,
                             InvoiceNo = SO.InvoiceNo,
                             ProductID = P.ProductID,
                             CategoryID = CAT.CategoryID,
                             CompanyID = COM.CompanyID,
                             ProductName = P.ProductName,
                             CategoryName = CAT.Description,
                             CompanyName = COM.Name,
                             Quantity = SOD.Quantity,
                             SalesRate = SOD.UnitPrice - SOD.PPDAmount,
                             TotalAmount = SOD.UTAmount,
                             IMEI = STD.IMENO
                         };

            return result.ToList();
        }

        public static decimal GetAllCollectionAmountByDateRange(this IBaseRepository<SOrder> SOrderRepository,
            IBaseRepository<CreditSale> CreditSaleRepository, IBaseRepository<CreditSalesSchedule> CreditSalesScheduleRepository,
            IBaseRepository<CashCollection> CashCollectionRepository, IBaseRepository<BankTransaction> BankTransactionRepository, DateTime fromDate, DateTime toDate)
        {
            decimal TotalCollection = 0m;

            var CashSales = SOrderRepository.All.Where(so => so.InvoiceDate >= fromDate && so.InvoiceDate <= toDate && so.Status == (int)EnumSalesType.Sales).ToList();
            if (CashSales.Count() > 0)
                TotalCollection += (decimal)CashSales.Sum(i => i.RecAmount);

            var Downpayment = CreditSaleRepository.All.Where(so => so.SalesDate >= fromDate && so.SalesDate <= toDate && so.IsStatus == EnumSalesType.Sales).ToList();
            if (Downpayment.Count() > 0)
                TotalCollection += (decimal)Downpayment.Sum(i => i.DownPayment);

            var InstallmentCollections = from so in CreditSaleRepository.All
                                         join css in CreditSalesScheduleRepository.All on so.CreditSalesID equals css.CreditSalesID
                                         where ((css.PaymentDate >= fromDate && css.PaymentDate <= toDate) && css.PaymentStatus.Equals("Paid") && so.IsStatus == EnumSalesType.Sales)
                                         select css;

            if (InstallmentCollections.ToList().Count() > 0)
                TotalCollection += (decimal)InstallmentCollections.Sum(i => i.InstallmentAmt);

            var CashCollections = CashCollectionRepository.All.Where(so => so.EntryDate >= fromDate && so.EntryDate <= toDate && so.TransactionType == EnumTranType.FromCustomer).ToList();
            if (CashCollections.Count() > 0)
                TotalCollection += (decimal)CashCollections.Sum(i => i.Amount);

            var BankCollections = BankTransactionRepository.All.Where(i => i.TranDate >= fromDate && i.TranDate <= toDate && i.CustomerID != 0).ToList();
            if (BankCollections.Count() > 0)
                TotalCollection += (decimal)BankCollections.Sum(i => i.Amount);

            return TotalCollection;
        }

        public static decimal GetVoltageStabilizerCommission(this IBaseRepository<SOrder> SOrderRepository, IBaseRepository<SOrderDetail> SOrderDetailRepository,
             IBaseRepository<CreditSale> CreditSaleRepository, IBaseRepository<CreditSaleDetails> CreditSaleDetailsRepository,
             IBaseRepository<Product> ProductRepository, IBaseRepository<ExtraCommissionSetup> ExtraCommissionSetupRepository,
             DateTime fromDate, DateTime toDate)
        {
            decimal TotalVSComm = 0m;
            var TargetCategory = ExtraCommissionSetupRepository.All.FirstOrDefault(i => i.Status == EnumCommissionType.VoltageStabilizerComm);
            var Sales = (from so in SOrderRepository.All
                         join sod in SOrderDetailRepository.All on so.SOrderID equals sod.SOrderID
                         join p in ProductRepository.All on sod.ProductID equals p.ProductID
                         where (so.Status == (int)EnumSalesType.Sales && so.InvoiceDate >= fromDate && so.InvoiceDate <= toDate) && (p.CategoryID == TargetCategory.CategoryID1 || p.CategoryID == TargetCategory.CategoryID2)
                         select new
                         {
                             so.InvoiceDate,
                             so.CustomerID,
                             sod.ProductID,
                             p.CategoryID
                         }).ToList();

            var CreditSales = (from so in CreditSaleRepository.All
                               join sod in CreditSaleDetailsRepository.All on so.CreditSalesID equals sod.CreditSalesID
                               join p in ProductRepository.All on sod.ProductID equals p.ProductID
                               where (so.IsStatus == EnumSalesType.Sales && so.SalesDate >= fromDate && so.SalesDate <= toDate) && (p.CategoryID == TargetCategory.CategoryID1 || p.CategoryID == TargetCategory.CategoryID2)
                               select new
                               {
                                   InvoiceDate = so.SalesDate,
                                   so.CustomerID,
                                   sod.ProductID,
                                   p.CategoryID
                               }).ToList();

            Sales.AddRange(CreditSales);

            var SalesVoltageStabilizerComm = (from so in Sales
                                              group so by new { so.InvoiceDate, so.CustomerID } into g
                                              select new
                                              {
                                                  InvoiceDate = g.Key.InvoiceDate,
                                                  CustomerID = g.Key.CustomerID,
                                                  Categories = g.Select(i => i.CategoryID).ToList()
                                              }).ToList();

            int Flag1 = 0, Flag2 = 0, Counter = 0;
            foreach (var item in SalesVoltageStabilizerComm)
            {
                if (item.Categories.Any(i => i == TargetCategory.CategoryID1))
                    Flag1++;
                if (item.Categories.Any(i => i == TargetCategory.CategoryID2))
                    Flag2++;
                if (Flag1 > 0 && Flag2 > 0)
                {
                    Counter++;
                }
                Flag1 = 0;
                Flag2 = 0;
            }

            TotalVSComm = 250m * Counter;

            return TotalVSComm;
        }

        /// <summary>
        /// Date: 25-02-2019
        /// Author: aminul
        /// </summary>
        public static decimal GetExtraCommission(this IBaseRepository<SOrder> SOrderRepository, IBaseRepository<SOrderDetail> SOrderDetailRepository,
                         IBaseRepository<CreditSale> CreditSaleRepository, IBaseRepository<CreditSaleDetails> CreditSaleDetailsRepository,
                         IBaseRepository<Product> ProductRepository, IBaseRepository<ExtraCommissionSetup> ExtraCommissionSetupRepository,
                         DateTime fromDate, DateTime toDate, int ConcernID)
        {
            decimal TotalExtraComm = 0m;

            if (ConcernID == (int)EnumSisterConcern.SAMSUNG_ELECTRA_CONCERNID)
            {
                var TargetCategory = ExtraCommissionSetupRepository.All.FirstOrDefault(i => i.Status == EnumCommissionType.ExtraComm);
                var Sales = (from so in SOrderRepository.All
                             join sod in SOrderDetailRepository.All on so.SOrderID equals sod.SOrderID
                             join p in ProductRepository.All on sod.ProductID equals p.ProductID
                             where (so.Status == (int)EnumSalesType.Sales && so.InvoiceDate >= fromDate && so.InvoiceDate <= toDate)
                             && ((p.CategoryID == TargetCategory.CategoryID1 && p.CompanyID == TargetCategory.CompanyID) || (p.CategoryID == TargetCategory.CategoryID2 && p.CompanyID == TargetCategory.CompanyID))
                             //&& sod.PPDAmount <= 250
                             select new
                             {
                                 so.InvoiceDate,
                                 so.CustomerID,
                                 sod.ProductID,
                                 p.CategoryID,
                                 sod.PPDAmount,
                                 sod.PPDPercentage,
                                 sod.UnitPrice,
                                 sod.Quantity,
                                 sod.UTAmount
                             }).ToList();
                decimal TotalSalesAmt = Sales.Sum(i => i.Quantity) * 1000m;
                decimal AcceptedAmount = (TotalSalesAmt * 25m) / 100m;
                decimal TotalGivenDiscount = Sales.Sum(i => (i.PPDAmount * i.Quantity));
                if (TotalGivenDiscount <= AcceptedAmount)
                    TotalExtraComm = 250m * Sales.Count();
            }
            else if (ConcernID == (int)EnumSisterConcern.HAWRA_ENTERPRISE_CONCERNID || ConcernID == (int)EnumSisterConcern.HAVEN_ENTERPRISE_CONCERNID)
            {
                var Creditsales6M = CreditSaleRepository.All.Where(i => i.InstallmentPeriod.ToLower().Equals("6 months")
                    && (i.SalesDate >= fromDate && i.SalesDate <= toDate && i.IsStatus == EnumSalesType.Sales)).ToList();
                if (Creditsales6M.Count > 0)
                    TotalExtraComm = Creditsales6M.Sum(i => i.NetAmount) * .0025m;

                var Creditsales12M = CreditSaleRepository.All.Where(i => i.InstallmentPeriod.ToLower().Equals("12 months")
                    && (i.SalesDate >= fromDate && i.SalesDate <= toDate && i.IsStatus == EnumSalesType.Sales)).ToList();
                if (Creditsales12M.Count() > 0)
                    TotalExtraComm += Creditsales12M.Sum(i => i.NetAmount) * .0050m;
            }


            return TotalExtraComm;
        }
        public static bool IsIMEIAlreadyReplaced(this IBaseRepository<SOrder> SOrderRepository,
         IBaseRepository<SOrderDetail> SOrderDetailRepo, int StockDetailID)
        {
            var RepORders = from so in SOrderRepository.All
                            join sod in SOrderDetailRepo.All on so.SOrderID equals sod.RepOrderID
                            where sod.RStockDetailID == StockDetailID && so.Status == (int)EnumSalesType.Sales && so.IsReplacement == 1
                            select sod;

            if (RepORders.Count() > 0)
                return true;
            else
                return false;
        }

        public static List<SOredersReportModel> GetAdminSalesReport(this IBaseRepository<SOrder> SOrderRepository, IBaseRepository<SOrderDetail> SOrderDetailRepository,
            IBaseRepository<Customer> CustomerRepository, IBaseRepository<SisterConcern> SisterConcernRepository,
            int ConcernID, DateTime fromDate, DateTime toDate)
        {
            IQueryable<Customer> Customers = null;
            if (ConcernID != 0)
                Customers = CustomerRepository.GetAll().Where(i => i.ConcernID == ConcernID);
            else
                Customers = CustomerRepository.GetAll();
            var Sales = (from so in SOrderRepository.GetAll()
                         join c in Customers on so.CustomerID equals c.CustomerID
                         join s in SisterConcernRepository.GetAll() on so.ConcernID equals s.ConcernID
                         where so.Status == (int)EnumSalesType.Sales && (so.InvoiceDate >= fromDate && so.InvoiceDate <= toDate) && so.IsReplacement != 1
                         select new SOredersReportModel
                         {
                             ConcernID = so.ConcernID,
                             ConcernName = s.Name,
                             CustomerCode = c.Code,
                             CustomerName = c.Name,
                             InvoiceDate = so.InvoiceDate,
                             InvoiceNo = so.InvoiceNo,
                             Grandtotal = so.GrandTotal,
                             NetDiscount = so.NetDiscount,
                             TotalOffer = 0,
                             AdjAmount = so.AdjAmount,
                             TotalAmount = so.TotalAmount,
                             RecAmount = (decimal)so.RecAmount,
                             PaymentDue = so.PaymentDue,
                             CustomerTotalDue = c.TotalDue
                         }).ToList();

            var Replaces = (from so in SOrderRepository.GetAll()
                            join c in Customers on so.CustomerID equals c.CustomerID
                            join s in SisterConcernRepository.GetAll() on so.ConcernID equals s.ConcernID
                            where so.Status == (int)EnumSalesType.Sales && (so.InvoiceDate >= fromDate && so.InvoiceDate <= toDate) && so.IsReplacement == 1
                            select new SOredersReportModel
                            {
                                ConcernID = so.ConcernID,
                                ConcernName = s.Name,
                                CustomerCode = c.Code,
                                CustomerName = c.Name,
                                InvoiceDate = so.InvoiceDate,
                                InvoiceNo = "REP-" + so.InvoiceNo,
                                Grandtotal = so.GrandTotal,
                                NetDiscount = so.NetDiscount,
                                TotalOffer = 0,
                                AdjAmount = so.AdjAmount,
                                TotalAmount = so.TotalAmount,
                                RecAmount = (decimal)so.RecAmount,
                                PaymentDue = so.PaymentDue,
                                CustomerTotalDue = c.TotalDue
                            }).ToList();
            Sales.AddRange(Replaces);
            return Sales.OrderBy(i => i.ConcernID).ThenByDescending(i => i.InvoiceDate).ToList();
        }


        public static List<LedgerAccountReportModel> CustomerLedger(this IBaseRepository<SOrder> SOrderRepository, IBaseRepository<SOrderDetail> SOrderDetailRepository, IBaseRepository<Customer> CustomerRepository, IBaseRepository<ApplicationUser> UserRepository, IBaseRepository<BankTransaction> BankTransactionRepository, IBaseRepository<CashCollection> CashCollectionRepository, IBaseRepository<CreditSale> CreditSaleRepository, IBaseRepository<CreditSaleDetails> CreditSaleDetailsRepo, IBaseRepository<CreditSalesSchedule> CreditSalesScheduleRepo, IBaseRepository<Product> ProductRepository, IBaseRepository<Bank> BankRepository, IBaseRepository<ROrder> RorderRepository, IBaseRepository<ROrderDetail> RoDerDetailRepository, int CustomerID, DateTime fromDate, DateTime toDate)
        {
            List<LedgerAccountReportModel> ledgers = new List<LedgerAccountReportModel>();
            List<LedgerAccountReportModel> FinalLedgers = new List<LedgerAccountReportModel>();

            var Customer = CustomerRepository.GetAll().FirstOrDefault(i => i.CustomerID == CustomerID);

            #region Cash Sales
            // Step 1: Get raw data without formatting
            var CashSalesRaw = (from so in SOrderRepository.All
                                join sod in SOrderDetailRepository.All on so.SOrderID equals sod.SOrderID
                                join p in ProductRepository.All on sod.ProductID equals p.ProductID
                                join u in UserRepository.All on so.CreatedBy equals u.Id into lj
                                from u in lj.DefaultIfEmpty()
                                where so.Status == (int)EnumSalesType.Sales && so.CustomerID == CustomerID
                                select new
                                {
                                    so.TotalAmount,
                                    so.InvoiceDate,
                                    so.InvoiceNo,
                                    so.RecAmount,
                                    CreditAdj = so.AdjAmount + so.NetDiscount,
                                    Credit = (decimal)so.RecAmount,
                                    CashCollectionAmt = (decimal)so.RecAmount,
                                    Debit = (decimal)(so.TotalAmount),
                                    GrandTotal = so.GrandTotal,
                                    LabourCost = so.LabourCost,
                                    sod.UnitPrice,
                                    sod.UTAmount,
                                    sod.Quantity,
                                    SRate = sod.SRate,
                                    ProductName = p.ProductName,
                                    EnteredBy = u == null ? string.Empty : u.UserName,
                                    Remarks = so.Remarks
                                }).ToList(); // Materialize the query

            // Step 2: Format values in-memory using C# string formatting
            var CashSales = CashSalesRaw.Select(x => new
            {
                x.TotalAmount,
                x.InvoiceDate,
                x.InvoiceNo,
                x.RecAmount,
                x.CreditAdj,
                x.Credit,
                x.CashCollectionAmt,
                x.Debit,
                x.GrandTotal,
                x.LabourCost,
                x.UnitPrice,
                x.UTAmount,
                x.Quantity,
                ProductName = $"{x.ProductName} {x.Quantity} {x.SRate:F2} {x.UTAmount:F2}",
                x.EnteredBy,
                x.Remarks
            });

            // Step 3: Group and create the final view model
            var VmCashSales = (from cs in CashSales
                               group cs by new
                               {
                                   cs.Debit,
                                   cs.Credit,
                                   cs.CreditAdj,
                                   cs.GrandTotal,
                                   cs.CashCollectionAmt,
                                   cs.InvoiceDate,
                                   cs.InvoiceNo,
                                   cs.EnteredBy
                               } into g
                               select new LedgerAccountReportModel
                               {
                                   VoucherType = "Sales",
                                   InvoiceNo = g.Key.InvoiceNo,
                                   Date = g.Key.InvoiceDate,
                                   EnteredBy = "Entered By: " + g.Key.EnteredBy,
                                   ProductList = g.Select(i => i.ProductName).ToList(),
                                   Debit = g.Key.Debit,
                                   Credit = g.Key.Credit,
                                   CreditAdj = g.Key.CreditAdj,
                                   GrandTotal = g.Key.GrandTotal,
                                   CashCollectionAmt = g.Key.CashCollectionAmt,
                                   Quantity = g.Sum(i => i.Quantity),
                                   Balance = 0,
                                   Remarks = g.Select(i => i.Remarks).FirstOrDefault(),
                                   LabourCost = g.Select(i => i.LabourCost).FirstOrDefault()
                               }).ToList();

            ledgers.AddRange(VmCashSales);

            //var CashSales = from so in SOrderRepository.All
            //                join sod in SOrderDetailRepository.All on so.SOrderID equals sod.SOrderID
            //                join p in ProductRepository.All on sod.ProductID equals p.ProductID
            //                join u in UserRepository.All on so.CreatedBy equals u.Id into lj
            //                from u in lj.DefaultIfEmpty()
            //                where so.Status == (int)EnumSalesType.Sales && so.CustomerID == CustomerID
            //                select new
            //                {
            //                    so.TotalAmount,
            //                    so.InvoiceDate,
            //                    so.InvoiceNo,
            //                    so.RecAmount,
            //                    CreditAdj = so.AdjAmount + so.NetDiscount,
            //                    Credit = (decimal)so.RecAmount,
            //                    CashCollectionAmt = (decimal)so.RecAmount,
            //                    Debit = (decimal)(so.TotalAmount),
            //                    GrandTotal = so.GrandTotal,
            //                    LabourCost = so.LabourCost,
            //                    sod.UnitPrice,
            //                    sod.UTAmount,
            //                    sod.Quantity,
            //                    ProductName = p.ProductName + " " + sod.Quantity.ToString() +  " " + sod.SRate.ToString("F2") + " " + sod.UTAmount.ToString("F2"),
            //                    EnteredBy = u == null ? string.Empty : u.UserName,
            //                    Remarks = so.Remarks,
            //                };

            //var VmCashSales = (from cs in CashSales
            //                   group cs by new { cs.Debit, cs.Credit, cs.CreditAdj, cs.GrandTotal, cs.CashCollectionAmt, cs.InvoiceDate, cs.InvoiceNo, cs.EnteredBy } into g
            //                   select new LedgerAccountReportModel
            //                   {
            //                       VoucherType = "Sales",
            //                       InvoiceNo = g.Key.InvoiceNo,
            //                       Date = g.Key.InvoiceDate,
            //                       EnteredBy = "Entered By: " + g.Key.EnteredBy,
            //                       ProductList = g.Select(i => i.ProductName).ToList(),
            //                       Debit = g.Key.Debit,
            //                       Credit = g.Key.Credit,
            //                       CreditAdj = g.Key.CreditAdj,
            //                       GrandTotal = g.Key.GrandTotal,
            //                       CashCollectionAmt = g.Key.CashCollectionAmt,
            //                       Quantity = g.Sum(i => i.Quantity),
            //                       Balance = 0,
            //                       Remarks = g.Select(i => i.Remarks).FirstOrDefault(),
            //                       LabourCost = g.Select(i => i.LabourCost).FirstOrDefault()
            //                   }).ToList();

            //ledgers.AddRange(VmCashSales);
            #endregion

            #region Cash Sales Return ROrders
            var CashSalesProductReturn = from so in RorderRepository.All
                                         join sod in RoDerDetailRepository.All on so.ROrderID equals sod.ROrderID
                                         join p in ProductRepository.All on sod.ProductID equals p.ProductID
                                         join u in UserRepository.All on so.CreatedBy equals u.Id into lj
                                         from u in lj.DefaultIfEmpty()
                                         where so.CustomerID == CustomerID
                                         //so.Status == (int)EnumSalesType.ProductReturn && 
                                         select new
                                         {
                                             TotalAmount = so.GrandTotal,
                                             InvoiceDate = so.ReturnDate,
                                             so.InvoiceNo,
                                             RecAmount = so.PaidAmount,
                                             AdjAmount = 0m,
                                             Credit = (decimal)(so.GrandTotal),
                                             Debit = (decimal)(so.PaidAmount),
                                             Return = so.PaidAmount,
                                             sod.UnitPrice,
                                             sod.UTAmount,
                                             sod.Quantity,
                                             ProductName = p.ProductName + " " + sod.Quantity.ToString() + " " + p.ProUnitTypeID.ToString() + " " + sod.UnitPrice + " " + sod.UTAmount,
                                             EnteredBy = u == null ? string.Empty : u.UserName,
                                             Remarks = so.Remarks,
                                         };

            var VmCashSalesProductReturn = (from cs in CashSalesProductReturn
                                            group cs by new { cs.Debit, cs.Credit, cs.Return, cs.InvoiceDate, cs.InvoiceNo, cs.EnteredBy } into g
                                            select new LedgerAccountReportModel
                                            {
                                                VoucherType = "Sales Return",
                                                InvoiceNo = g.Key.InvoiceNo,
                                                Date = g.Key.InvoiceDate,
                                                EnteredBy = "Entered By: " + g.Key.EnteredBy,
                                                ProductList = g.Select(i => i.ProductName).ToList(),
                                                Debit = g.Key.Debit,
                                                Credit = g.Key.Credit,
                                                SalesReturn = g.Key.Return,
                                                Quantity = g.Sum(i => i.Quantity),
                                                Balance = 0,
                                                Remarks = g.Select(i => i.Remarks).FirstOrDefault()
                                            }).ToList();

            ledgers.AddRange(VmCashSalesProductReturn);
            #endregion

            #region Cash Sales Return
            var CashSalesReturn = from so in SOrderRepository.All
                                  join sod in SOrderDetailRepository.All on so.SOrderID equals sod.SOrderID
                                  join p in ProductRepository.All on sod.ProductID equals p.ProductID
                                  join u in UserRepository.All on so.CreatedBy equals u.Id into lj
                                  from u in lj.DefaultIfEmpty()
                                  where so.Status == (int)EnumSalesType.ProductReturn && so.CustomerID == CustomerID
                                  select new
                                  {
                                      so.TotalAmount,
                                      so.InvoiceDate,
                                      so.InvoiceNo,
                                      so.RecAmount,
                                      so.AdjAmount,
                                      Credit = (decimal)(so.TotalAmount),
                                      Debit = (decimal)(so.RecAmount),
                                      Return = (decimal)(so.TotalAmount - so.RecAmount),
                                      sod.UnitPrice,
                                      sod.UTAmount,
                                      sod.Quantity,
                                      ProductName = p.ProductName + " " + sod.Quantity.ToString() + " " + p.ProUnitTypeID.ToString() + " " + sod.SRate + " " + sod.UTAmount,
                                      EnteredBy = u == null ? string.Empty : u.UserName,
                                      Remarks = so.Remarks,
                                  };

            var VmCashSalesReturn = (from cs in CashSalesReturn
                                     group cs by new { cs.Debit, cs.Credit, cs.Return, cs.InvoiceDate, cs.InvoiceNo, cs.EnteredBy } into g
                                     select new LedgerAccountReportModel
                                     {
                                         VoucherType = "Sales Return",
                                         InvoiceNo = g.Key.InvoiceNo,
                                         Date = g.Key.InvoiceDate,
                                         EnteredBy = "Entered By: " + g.Key.EnteredBy,
                                         ProductList = g.Select(i => i.ProductName).ToList(),
                                         Debit = g.Key.Debit,
                                         Credit = g.Key.Credit,
                                         SalesReturn = g.Key.Return,
                                         Quantity = g.Sum(i => i.Quantity),
                                         Balance = 0,
                                         Remarks = g.Select(i => i.Remarks).FirstOrDefault()
                                     }).ToList();

            ledgers.AddRange(VmCashSalesReturn);
            #endregion

            #region Credit Sales
            var CreditSales = from so in CreditSaleRepository.All
                              join sod in CreditSaleDetailsRepo.All on so.CreditSalesID equals sod.CreditSalesID
                              join p in ProductRepository.All on sod.ProductID equals p.ProductID
                              join u in UserRepository.All on so.CreatedBy equals u.Id into lj
                              from u in lj.DefaultIfEmpty()
                              where so.IsStatus == EnumSalesType.Sales && so.CustomerID == CustomerID
                              select new
                              {
                                  so.NetAmount,
                                  so.SalesDate,
                                  so.InvoiceNo,
                                  CreditAdj = so.Discount,
                                  Credit = so.DownPayment,
                                  CashCollectionAmt = so.DownPayment,
                                  Debit = so.NetAmount,
                                  GrandTotal = so.TSalesAmt,
                                  sod.UnitPrice,
                                  sod.UTAmount,
                                  sod.Quantity,
                                  ProductName = p.ProductName + " " + sod.Quantity.ToString() + " " + p.ProUnitTypeID.ToString() + " " + sod.UnitPrice + " " + sod.UTAmount,
                                  EnteredBy = u == null ? string.Empty : u.UserName,
                                  Remarks = so.Remarks,
                              };

            var VmCreditSales = (from cs in CreditSales
                                 group cs by new { cs.Debit, cs.Credit, cs.GrandTotal, cs.CashCollectionAmt, cs.SalesDate, cs.InvoiceNo, cs.EnteredBy } into g
                                 select new LedgerAccountReportModel
                                 {
                                     VoucherType = "Hire Sales",
                                     InvoiceNo = g.Key.InvoiceNo,
                                     Date = g.Key.SalesDate,
                                     EnteredBy = "Entered By: " + g.Key.EnteredBy,
                                     ProductList = g.Select(i => i.ProductName).ToList(),
                                     Debit = g.Key.Debit,
                                     Credit = g.Key.Credit,
                                     CashCollectionAmt = g.Key.CashCollectionAmt,
                                     GrandTotal = g.Key.GrandTotal,
                                     Quantity = g.Sum(i => i.Quantity),
                                     Balance = 0,
                                     Remarks = g.Select(i => i.Remarks).FirstOrDefault()
                                 }).ToList();

            ledgers.AddRange(VmCreditSales);
            #endregion

            #region Installment Collection
            var CreditSchedule = from so in CreditSaleRepository.All
                                 join sod in CreditSalesScheduleRepo.All on so.CreditSalesID equals sod.CreditSalesID
                                 where so.IsStatus == EnumSalesType.Sales && sod.PaymentStatus == "Paid" && so.CustomerID == CustomerID && sod.InstallmentAmt != 0
                                 select new LedgerAccountReportModel
                                 {
                                     VoucherType = "Installment",
                                     InvoiceNo = so.InvoiceNo + "-" + sod.ScheduleNo,
                                     Date = sod.PaymentDate,
                                     Debit = 0m,
                                     Quantity = 0m,
                                     Credit = sod.InstallmentAmt + 0,//sod.LastPayAdjust,
                                     CashCollectionAmt = sod.InstallmentAmt,
                                     CreditAdj = 0,//sod.LastPayAdjust,
                                     Balance = 0,
                                     Remarks = sod.Remarks
                                 };
            ledgers.AddRange(CreditSchedule);
            #endregion

            #region Cash Collection
            var CashCollection = from cc in CashCollectionRepository.All
                                 join u in UserRepository.All on cc.CreatedBy equals u.Id into lj
                                 from u in lj.DefaultIfEmpty()
                                 where cc.CustomerID == CustomerID && cc.TransactionType == EnumTranType.FromCustomer
                                 select new LedgerAccountReportModel
                                 {
                                     Date = (DateTime)cc.EntryDate,
                                     Debit = cc.InterestAmt,
                                     VoucherType = "Cash Collection",
                                     Credit = cc.Amount + cc.AdjustAmt,
                                     CashCollectionAmt = cc.Amount,
                                     CreditAdj = cc.AdjustAmt,
                                     InvoiceNo = cc.ReceiptNo,
                                     EnteredBy = "Entered By: " + u.UserName,
                                     Remarks = cc.Remarks,
                                     CashCollectionIntAmt = cc.InterestAmt
                                 };
            ledgers.AddRange(CashCollection);
            #endregion

            #region Cash Collection Return
            var CashCollectionReturn = from ccr in CashCollectionRepository.All
                                       join u in UserRepository.All on ccr.CreatedBy equals u.Id into lj
                                       from u in lj.DefaultIfEmpty()
                                       where ccr.CustomerID == CustomerID && ccr.TransactionType == EnumTranType.CollectionReturn
                                       select new LedgerAccountReportModel
                                       {
                                           Date = (DateTime)ccr.EntryDate,
                                           Credit = 0m,
                                           VoucherType = "Cash Collection Return",
                                           Debit = ccr.Amount + ccr.AdjustAmt,
                                           CashCollectionReturn = ccr.Amount,
                                           CreditAdj = ccr.AdjustAmt,
                                           InvoiceNo = ccr.ReceiptNo,
                                           EnteredBy = "Entered By: " + u.UserName,
                                           Remarks = ccr.Remarks
                                       };
            ledgers.AddRange(CashCollectionReturn);
            #endregion

            #region Bank Transaction
            var bankTrans = from bt in BankTransactionRepository.All
                            join b in BankRepository.All on bt.BankID equals b.BankID
                            where bt.CustomerID == CustomerID && bt.SorderID == 0
                            select new LedgerAccountReportModel
                            {
                                Date = (DateTime)bt.TranDate,
                                Debit = 0m,
                                VoucherType = "Bank Collect.",
                                Credit = bt.Amount,
                                CashCollectionAmt = bt.Amount,
                                CreditAdj = 0m,
                                InvoiceNo = bt.TransactionNo,
                                Particulars = b.AccountName + " " + b.AccountNo + " Chk. No: " + bt.ChecqueNo,
                                Remarks = bt.Remarks
                            };
            ledgers.AddRange(bankTrans);
            #endregion

            decimal balance = Customer.OpeningDue;
            ledgers = ledgers.OrderBy(i => i.Date).ToList();
            foreach (var item in ledgers)
            {
                item.Balance = balance + (item.Debit - item.Credit);
                item.Particulars = string.IsNullOrEmpty(item.Particulars) ? string.Join(Environment.NewLine, item.ProductList) + Environment.NewLine + item.EnteredBy : item.Particulars;
                balance = item.Balance;
            }

            var oOpening = new LedgerAccountReportModel() { Date = new DateTime(2015, 1, 1), Particulars = "Opening Balance", Debit = Customer.OpeningDue, Balance = 0, Credit = 0 };

            if (ledgers.Count > 0)
            {
                decimal openingBalance = Customer.OpeningDue;
                var priorTrans = ledgers.Where(i => i.Date < fromDate).ToList();
                if (priorTrans.Any())
                    openingBalance = priorTrans.Last().Balance;

                FinalLedgers.Add(new LedgerAccountReportModel
                {
                    Date = fromDate,
                    Particulars = "Opening Balance",
                    Balance = openingBalance,
                    Debit = 0m,
                    Credit = 0m
                });

                FinalLedgers.AddRange(ledgers
                    .Where(i => i.Date >= fromDate && i.Date <= toDate)
                    .OrderBy(i => i.Date)
                    .ToList());

                //ledgers.Insert(0, oOpening);
                //var OpeningTrans = ledgers.Where(i => i.Date < fromDate).OrderByDescending(i => i.Date).FirstOrDefault();
                //if (OpeningTrans != null)
                //    FinalLedgers.Add(new LedgerAccountReportModel() { Date = OpeningTrans.Date, Particulars = "Opening Balance", Balance = OpeningTrans.Balance, Debit = 0m });
                //else
                //    FinalLedgers.Add(new LedgerAccountReportModel() { Date = fromDate, Particulars = "Opening Balance", Balance = Customer.OpeningDue, Debit = 0m });

                //ledgers = ledgers.Where(i => i.Date >= fromDate && i.Date <= toDate).OrderBy(i => i.Date).ToList();
                //FinalLedgers.AddRange(ledgers);
            }
            else
            {
                FinalLedgers.Add(new LedgerAccountReportModel() { Date = fromDate, Particulars = "Opening Balance", Debit = Customer.OpeningDue, Credit = 0m, Balance = Customer.OpeningDue });
            }

            return FinalLedgers;
        }

        public static List<LedgerAccountReportModel> BothCustomerLedger(this IBaseRepository<SOrder> SOrderRepository, IBaseRepository<SOrderDetail> SOrderDetailRepository, IBaseRepository<Customer> CustomerRepository, IBaseRepository<ApplicationUser> UserRepository, IBaseRepository<BankTransaction> BankTransactionRepository, IBaseRepository<CashCollection> CashCollectionRepository, IBaseRepository<CreditSale> CreditSaleRepository, IBaseRepository<CreditSaleDetails> CreditSaleDetailsRepo, IBaseRepository<CreditSalesSchedule> CreditSalesScheduleRepo, IBaseRepository<Product> ProductRepository, IBaseRepository<Bank> BankRepository, IBaseRepository<ROrder> RorderRepository, IBaseRepository<ROrderDetail> RoDerDetailRepository, IBaseRepository<Supplier> SupplierRepository, IBaseRepository<POrder> POrderRepository, IBaseRepository<POrderDetail> POrderDetailRepository, IBaseRepository<POProductDetail> POProductDetailRepository, int CustomerID, DateTime fromDate, DateTime toDate)
        {
            List<LedgerAccountReportModel> ledgers = new List<LedgerAccountReportModel>();
            List<LedgerAccountReportModel> FinalLedgers = new List<LedgerAccountReportModel>();

            var Customer = CustomerRepository.GetAll().FirstOrDefault(i => i.CustomerID == CustomerID);
            var Supplier = SupplierRepository.GetAll().FirstOrDefault(i => i.CustomerID == CustomerID);


            #region Purchase
            var Purchases = from po in POrderRepository.All
                            join pod in POrderDetailRepository.All on po.POrderID equals pod.POrderID
                            join p in ProductRepository.All on pod.ProductID equals p.ProductID
                            where po.Status == (int)EnumPurchaseType.Purchase && po.SupplierID == Supplier.SupplierID
                            select new
                            {
                                po.TotalAmt,
                                po.ChallanNo,
                                po.OrderDate,
                                po.RecAmt,
                                DebitAdj = po.AdjAmount + po.NetDiscount,
                                Debit = po.RecAmt,
                                CashCollectionAmt = po.RecAmt,
                                Credit = po.TotalAmt,
                                GrandTotal = po.GrandTotal,
                                pod.Quantity,
                                ProductName = p.ProductName + " " + pod.Quantity.ToString() + " " + p.ProUnitTypeID.ToString(),
                                Remarks = string.Empty,
                            };

            var VmPurchases = (from cs in Purchases
                               group cs by new { cs.Debit, cs.Credit, cs.DebitAdj, cs.GrandTotal, cs.CashCollectionAmt, cs.OrderDate, cs.ChallanNo } into g
                               select new LedgerAccountReportModel
                               {
                                   VoucherType = "Purchase",
                                   InvoiceNo = g.Key.ChallanNo,
                                   Date = g.Key.OrderDate,
                                   ProductList = g.Select(i => i.ProductName).ToList(),
                                   Debit = g.Key.Debit,
                                   Credit = g.Key.Credit,
                                   DebitAdj = g.Key.DebitAdj,
                                   GrandTotal = g.Key.GrandTotal,
                                   CashCollectionAmt = g.Key.CashCollectionAmt,
                                   Quantity = g.Sum(i => i.Quantity),
                                   Balance = 0,
                                   Remarks = g.Select(i => i.Remarks).FirstOrDefault()
                               }).ToList();

            ledgers.AddRange(VmPurchases);
            #endregion

            #region Purchases Return
            //var PurchasesReturns = from po in POrderRepository.All
            //                       join pod in POrderDetailRepository.All on po.POrderID equals pod.POrderID
            //                       join p in ProductRepository.All on pod.ProductID equals p.ProductID                                   
            //                       where po.Status == (int)EnumPurchaseType.ProductReturn && po.SupplierID == Supplier.SupplierID
            //                       select new
            //                       {
            //                           po.TotalAmt,
            //                           po.ChallanNo,
            //                           po.OrderDate,
            //                           po.RecAmt,
            //                           CreditAdj = po.AdjAmount + po.NetDiscount,
            //                           Credit = po.TotalAmt,
            //                           Return = po.TotalAmt - po.RecAmt,
            //                           CashCollectionAmt = po.RecAmt,
            //                           Debit = po.RecAmt,
            //                           GrandTotal = po.GrandTotal,
            //                           pod.UnitPrice,
            //                           pod.TAmount,
            //                           pod.Quantity,
            //                           ProductName = p.ProductName + " " + pod.Quantity.ToString() + " " + p.ProUnitTypeID.ToString() + " " + pod.UnitPrice.ToString() + " " + pod.TAmount.ToString(),                                       
            //                           Remarks = string.Empty,
            //                       };

            //var VmPurchasesReturns = (from cs in PurchasesReturns
            //                          group cs by new { cs.Debit, cs.Credit, cs.Return, cs.OrderDate, cs.ChallanNo, cs.EnteredBy } into g
            //                          select new LedgerAccountReportModel
            //                          {
            //                              VoucherType = "PO Return",
            //                              InvoiceNo = g.Key.ChallanNo,
            //                              Date = g.Key.OrderDate,                                          
            //                              ProductList = g.Select(i => i.ProductName).ToList(),
            //                              Debit = g.Key.Debit,
            //                              Credit = g.Key.Credit,
            //                              SalesReturn = g.Key.Return,
            //                              Quantity = g.Sum(i => i.Quantity),
            //                              Balance = 0,
            //                              Remarks = g.Select(i => i.Remarks).FirstOrDefault()
            //                          }).ToList();

            //ledgers.AddRange(VmPurchasesReturns);
            #endregion

            #region Cash Delivery
            var CashDelivery = from cc in CashCollectionRepository.All
                               where cc.SupplierID == Supplier.SupplierID
                               select new LedgerAccountReportModel
                               {
                                   Date = (DateTime)cc.EntryDate,
                                   Debit = cc.Amount + cc.AdjustAmt,
                                   VoucherType = "Cash Delivery",
                                   Credit = 0m,
                                   CashCollectionAmt = cc.Amount,
                                   CreditAdj = 0m,
                                   InvoiceNo = cc.ReceiptNo,
                                   Remarks = cc.Remarks
                               };
            ledgers.AddRange(CashDelivery);
            #endregion

            #region Bank Transaction
            var bankTrans = from bt in BankTransactionRepository.All
                            join b in BankRepository.All on bt.BankID equals b.BankID
                            where bt.SupplierID == Supplier.SupplierID || bt.CustomerID == CustomerID
                            select new LedgerAccountReportModel
                            {
                                Date = (DateTime)bt.TranDate,
                                Debit = bt.TransactionType == 4 ? bt.Amount: 0m,
                                VoucherType = bt.TransactionType == 4 ? "Bank Cash Delivery" :
                                                bt.TransactionType == 3 ? "Bank Cash Collection" :"",
                                Credit = bt.TransactionType == 3 ? bt.Amount : 0m,
                                CashCollectionAmt = bt.Amount,
                                CreditAdj = 0m,
                                InvoiceNo = bt.TransactionNo,
                                Particulars = b.AccountName + " " + b.AccountNo + " Chk. No: " + bt.ChecqueNo,
                                Remarks = bt.Remarks
                            };
            ledgers.AddRange(bankTrans);
            #endregion


            #region Cash Sales
            var CashSales = from so in SOrderRepository.All
                            join sod in SOrderDetailRepository.All on so.SOrderID equals sod.SOrderID
                            join p in ProductRepository.All on sod.ProductID equals p.ProductID

                            where so.Status == (int)EnumSalesType.Sales && so.CustomerID == CustomerID
                            select new
                            {
                                so.TotalAmount,
                                so.InvoiceDate,
                                so.InvoiceNo,
                                so.RecAmount,
                                CreditAdj = so.AdjAmount + so.NetDiscount,
                                Credit = (decimal)so.RecAmount,
                                CashCollectionAmt = (decimal)so.RecAmount,
                                Debit = (decimal)(so.TotalAmount),
                                GrandTotal = so.GrandTotal,
                                LabourCost = so.LabourCost,
                                sod.UnitPrice,
                                sod.UTAmount,
                                sod.Quantity,
                                ProductName = p.ProductName + " " + sod.Quantity.ToString() + " " + p.ProUnitTypeID.ToString() + " " + sod.SRate.ToString() + " " + sod.UTAmount.ToString(),

                                Remarks = so.Remarks,
                            };

            var VmCashSales = (from cs in CashSales
                               group cs by new { cs.Debit, cs.Credit, cs.CreditAdj, cs.GrandTotal, cs.CashCollectionAmt, cs.InvoiceDate, cs.InvoiceNo } into g
                               select new LedgerAccountReportModel
                               {
                                   VoucherType = "Sales",
                                   InvoiceNo = g.Key.InvoiceNo,
                                   Date = g.Key.InvoiceDate,
                                   ProductList = g.Select(i => i.ProductName).ToList(),
                                   Debit = g.Key.Debit,
                                   Credit = g.Key.Credit,
                                   CreditAdj = g.Key.CreditAdj,
                                   GrandTotal = g.Key.GrandTotal,
                                   CashCollectionAmt = g.Key.CashCollectionAmt,
                                   Quantity = g.Sum(i => i.Quantity),
                                   Balance = 0,
                                   Remarks = g.Select(i => i.Remarks).FirstOrDefault(),
                                   LabourCost = g.Select(i => i.LabourCost).FirstOrDefault()
                               }).ToList();

            ledgers.AddRange(VmCashSales);
            #endregion

            #region Cash Sales Return ROrders
            //var CashSalesProductReturn = from so in RorderRepository.All
            //                             join sod in RoDerDetailRepository.All on so.ROrderID equals sod.ROrderID
            //                             join p in ProductRepository.All on sod.ProductID equals p.ProductID
            //                             join u in UserRepository.All on so.CreatedBy equals u.Id into lj
            //                             from u in lj.DefaultIfEmpty()
            //                             where so.CustomerID == CustomerID
            //                             //so.Status == (int)EnumSalesType.ProductReturn && 
            //                             select new
            //                             {
            //                                 TotalAmount = so.GrandTotal,
            //                                 InvoiceDate = so.ReturnDate,
            //                                 so.InvoiceNo,
            //                                 RecAmount = so.PaidAmount,
            //                                 AdjAmount = 0m,
            //                                 Credit = (decimal)(so.GrandTotal),
            //                                 Debit = (decimal)(so.PaidAmount),
            //                                 Return = so.PaidAmount,
            //                                 sod.UnitPrice,
            //                                 sod.UTAmount,
            //                                 sod.Quantity,
            //                                 ProductName = p.ProductName + " " + sod.Quantity.ToString() + " " + p.ProUnitTypeID.ToString() + " " + sod.UnitPrice + " " + sod.UTAmount,
            //                                 EnteredBy = u == null ? string.Empty : u.UserName,
            //                                 Remarks = so.Remarks,
            //                             };

            //var VmCashSalesProductReturn = (from cs in CashSalesProductReturn
            //                                group cs by new { cs.Debit, cs.Credit, cs.Return, cs.InvoiceDate, cs.InvoiceNo, cs.EnteredBy } into g
            //                                select new LedgerAccountReportModel
            //                                {
            //                                    VoucherType = "Sales Return",
            //                                    InvoiceNo = g.Key.InvoiceNo,
            //                                    Date = g.Key.InvoiceDate,
            //                                    EnteredBy = "Entered By: " + g.Key.EnteredBy,
            //                                    ProductList = g.Select(i => i.ProductName).ToList(),
            //                                    Debit = g.Key.Debit,
            //                                    Credit = g.Key.Credit,
            //                                    SalesReturn = g.Key.Return,
            //                                    Quantity = g.Sum(i => i.Quantity),
            //                                    Balance = 0,
            //                                    Remarks = g.Select(i => i.Remarks).FirstOrDefault()
            //                                }).ToList();

            //ledgers.AddRange(VmCashSalesProductReturn);
            #endregion

            #region Cash Sales Return
            //var CashSalesReturn = from so in SOrderRepository.All
            //                      join sod in SOrderDetailRepository.All on so.SOrderID equals sod.SOrderID
            //                      join p in ProductRepository.All on sod.ProductID equals p.ProductID
            //                      join u in UserRepository.All on so.CreatedBy equals u.Id into lj
            //                      from u in lj.DefaultIfEmpty()
            //                      where so.Status == (int)EnumSalesType.ProductReturn && so.CustomerID == CustomerID
            //                      select new
            //                      {
            //                          so.TotalAmount,
            //                          so.InvoiceDate,
            //                          so.InvoiceNo,
            //                          so.RecAmount,
            //                          so.AdjAmount,
            //                          Credit = (decimal)(so.TotalAmount),
            //                          Debit = (decimal)(so.RecAmount),
            //                          Return = (decimal)(so.TotalAmount - so.RecAmount),
            //                          sod.UnitPrice,
            //                          sod.UTAmount,
            //                          sod.Quantity,
            //                          ProductName = p.ProductName + " " + sod.Quantity.ToString() + " " + p.ProUnitTypeID.ToString() + " " + sod.SRate + " " + sod.UTAmount,
            //                          EnteredBy = u == null ? string.Empty : u.UserName,
            //                          Remarks = so.Remarks,
            //                      };

            //var VmCashSalesReturn = (from cs in CashSalesReturn
            //                         group cs by new { cs.Debit, cs.Credit, cs.Return, cs.InvoiceDate, cs.InvoiceNo, cs.EnteredBy } into g
            //                         select new LedgerAccountReportModel
            //                         {
            //                             VoucherType = "Sales Return",
            //                             InvoiceNo = g.Key.InvoiceNo,
            //                             Date = g.Key.InvoiceDate,
            //                             EnteredBy = "Entered By: " + g.Key.EnteredBy,
            //                             ProductList = g.Select(i => i.ProductName).ToList(),
            //                             Debit = g.Key.Debit,
            //                             Credit = g.Key.Credit,
            //                             SalesReturn = g.Key.Return,
            //                             Quantity = g.Sum(i => i.Quantity),
            //                             Balance = 0,
            //                             Remarks = g.Select(i => i.Remarks).FirstOrDefault()
            //                         }).ToList();

            //ledgers.AddRange(VmCashSalesReturn);
            #endregion


            #region Cash Collection
            var CashCollection = from cc in CashCollectionRepository.All
                                 join u in UserRepository.All on cc.CreatedBy equals u.Id into lj
                                 from u in lj.DefaultIfEmpty()
                                 where cc.CustomerID == CustomerID && cc.TransactionType == EnumTranType.FromCustomer
                                 select new LedgerAccountReportModel
                                 {
                                     Date = (DateTime)cc.EntryDate,
                                     Debit = cc.InterestAmt,
                                     VoucherType = "Cash Collection",
                                     Credit = cc.Amount + cc.AdjustAmt,
                                     CashCollectionAmt = cc.Amount,
                                     CreditAdj = cc.AdjustAmt,
                                     InvoiceNo = cc.ReceiptNo,
                                     EnteredBy = "Entered By: " + u.UserName,
                                     Remarks = cc.Remarks,
                                     CashCollectionIntAmt = cc.InterestAmt
                                 };
            ledgers.AddRange(CashCollection);
            #endregion

            #region Cash Collection Return
            //var CashCollectionReturn = from ccr in CashCollectionRepository.All
            //                           join u in UserRepository.All on ccr.CreatedBy equals u.Id into lj
            //                           from u in lj.DefaultIfEmpty()
            //                           where ccr.CustomerID == CustomerID && ccr.TransactionType == EnumTranType.CollectionReturn
            //                           select new LedgerAccountReportModel
            //                           {
            //                               Date = (DateTime)ccr.EntryDate,
            //                               Credit = 0m,
            //                               VoucherType = "Cash Collection Return",
            //                               Debit = ccr.Amount + ccr.AdjustAmt,
            //                               CashCollectionReturn = ccr.Amount,
            //                               CreditAdj = ccr.AdjustAmt,
            //                               InvoiceNo = ccr.ReceiptNo,
            //                               EnteredBy = "Entered By: " + u.UserName,
            //                               Remarks = ccr.Remarks
            //                           };
            //ledgers.AddRange(CashCollectionReturn);
            //#endregion

            //#region Bank Transaction
            //var bankTrans = from bt in BankTransactionRepository.All
            //                join b in BankRepository.All on bt.BankID equals b.BankID
            //                where bt.CustomerID == CustomerID && bt.SorderID == 0
            //                select new LedgerAccountReportModel
            //                {
            //                    Date = (DateTime)bt.TranDate,
            //                    Debit = 0m,
            //                    VoucherType = "Bank Collect.",
            //                    Credit = bt.Amount,
            //                    CashCollectionAmt = bt.Amount,
            //                    CreditAdj = 0m,
            //                    InvoiceNo = bt.TransactionNo,
            //                    Particulars = b.AccountName + " " + b.AccountNo + " Chk. No: " + bt.ChecqueNo,
            //                    Remarks = bt.Remarks
            //                };
            //ledgers.AddRange(bankTrans);
            #endregion

            decimal balance = Customer.OpeningDue == 0 ? Supplier.OpeningDue : Customer.OpeningDue;

            ledgers = ledgers.OrderBy(i => i.Date).ToList();
            foreach (var item in ledgers)
            {
                if (Customer.OpeningDue == 0)
                {
                    item.Balance = balance - (item.Debit - item.Credit);
                }
                else
                {
                    item.Balance = balance + (item.Debit - item.Credit);
                }

                item.Particulars = string.IsNullOrEmpty(item.Particulars) ? string.Join(Environment.NewLine, item.ProductList) + Environment.NewLine + item.EnteredBy : item.Particulars;
                balance = item.Balance;
            }

            var oOpening = new LedgerAccountReportModel() { Date = new DateTime(2015, 1, 1), Particulars = "Opening Balance", Debit = Customer.OpeningDue == 0 ? Supplier.OpeningDue : Customer.OpeningDue, Balance = 0, Credit = 0 };

            if (ledgers.Count > 0)
            {
                //ledgers.Insert(0, oOpening);
                var OpeningTrans = ledgers.Where(i => i.Date < fromDate).OrderByDescending(i => i.Date).FirstOrDefault();
                if (OpeningTrans != null)
                    FinalLedgers.Add(new LedgerAccountReportModel() { Date = OpeningTrans.Date, Particulars = "Opening Balance", Balance = OpeningTrans.Balance, Debit = 0m });
                else
                    FinalLedgers.Add(new LedgerAccountReportModel() { Date = fromDate, Particulars = "Opening Balance", Balance = Customer.OpeningDue == 0 ? Supplier.OpeningDue : Customer.OpeningDue, Debit = 0m });

                ledgers = ledgers.Where(i => i.Date >= fromDate && i.Date <= toDate).OrderBy(i => i.Date).ToList();
                FinalLedgers.AddRange(ledgers);
            }
            else
            {
                FinalLedgers.Add(new LedgerAccountReportModel() { Date = fromDate, Particulars = "Opening Balance", Debit = Customer.OpeningDue, Credit = 0m, Balance = Customer.OpeningDue == 0 ? Supplier.OpeningDue : Customer.OpeningDue });
            }

            return FinalLedgers;
        }




        public static IEnumerable<SOredersReportModel> GetforSalesReportForAll(
       this IBaseRepository<SOrder> salesOrderRepository, IBaseRepository<Customer> customerRepository,
       IBaseRepository<Employee> EmployeeRepository,
       DateTime fromDate, DateTime toDate, int EmployeeID, EnumCustomerType customerType)
        {
            IQueryable<Customer> Customers = null;
            if (EmployeeID > 0)
                Customers = customerRepository.All.Where(i => i.EmployeeID == EmployeeID);
            else
                Customers = customerRepository.All;


            var oSalesData = (from sord in salesOrderRepository.All
                              join cus in Customers on sord.CustomerID equals cus.CustomerID
                              join emp in EmployeeRepository.All on cus.EmployeeID equals emp.EmployeeID
                              where (sord.InvoiceDate >= fromDate && sord.InvoiceDate <= toDate && sord.Status == (int)EnumSalesType.Sales)
                              select new SOredersReportModel
                              {
                                  CustomerCode = cus.Code,
                                  CustomerName = cus.Name,
                                  CustomerAddress = cus.Address,
                                  CustomerContactNo = cus.ContactNo,
                                  InvoiceDate = sord.InvoiceDate,
                                  InvoiceNo = sord.InvoiceNo,
                                  Grandtotal = sord.GrandTotal,
                                  FlatDiscount = sord.TDAmount,
                                  TotalAmount = sord.TotalAmount,
                                  RecAmount = (decimal)sord.RecAmount,
                                  PaymentDue = sord.PaymentDue,
                                  CustomerID = sord.CustomerID,
                                  CustomerTotalDue = cus.TotalDue,
                                  EmployeeName = emp.Name,
                                  CustomerType = cus.CustomerType == EnumCustomerType.Dealer
                                  ? cus.CustomerType : EnumCustomerType.Retail

                              }).ToList();
            return oSalesData;
        }

    }
}
