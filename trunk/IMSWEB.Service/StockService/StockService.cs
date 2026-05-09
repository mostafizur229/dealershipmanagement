using IMSWEB.Data;
using IMSWEB.Data.Repositories.StockRepository;
using IMSWEB.Model;
using IMSWEB.Model.SPModel;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSWEB.Service
{
    public class StockService : IStockService
    {
        private readonly IBaseRepository<Stock> _stockRepository;
        private readonly IStockRepository _stockRepo;
        private readonly IBaseRepository<StockDetail> _stockDetailRepository;
        private readonly IBaseRepository<Product> _productRepository;
        private readonly IBaseRepository<Color> _ColorRepository;
        private readonly IBaseRepository<Supplier> _SupplierRepository;
        private readonly IBaseRepository<PriceProtection> _PriceProtectionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBaseRepository<SRVisit> _SRVisitRepository;
        private readonly IBaseRepository<SRVisitDetail> _SRVisitDetailRepository;
        private readonly IBaseRepository<SRVProductDetail> _SRVProductDetailRepository;
        private readonly IBaseRepository<Employee> _EmployeeRepository;
        private readonly IBaseRepository<Godown> _GodownRepository;
        private readonly IBaseRepository<POrder> _POrderRepository;
        private readonly IBaseRepository<POrderDetail> _POrderDetailRepository;
        private readonly IBaseRepository<Category> _CategoryRepository;
        private readonly IBaseRepository<Company> _CompanyRepository;
        private readonly IBaseRepository<SOrder> _SOrderRepository;
        private readonly IBaseRepository<SOrderDetail> _SOrderDetailRepository;
        private readonly IBaseRepository<CreditSale> _CreditSaleRepository;
        private readonly IBaseRepository<CreditSaleDetails> _CreditSaleDetailsRepository;
        private readonly IBaseRepository<ProductUnitType> _ProductUnitTypeRepository;
        private readonly IBaseRepository<Size> _SizeRepository;
        private readonly IBaseRepository<ROrder> _RorderRepository;
        private readonly IBaseRepository<ROrderDetail> _ROrderDetailRepository;
        public StockService(IBaseRepository<Stock> stockRepository, IBaseRepository<StockDetail> stockDetailRepository, IUnitOfWork unitOfWork,
            IBaseRepository<Product> productRepository, IBaseRepository<Color> colorRepository, IBaseRepository<Supplier> suppRepository,
            IBaseRepository<PriceProtection> priceProtectionRepository, IStockRepository stockRepo,
            IBaseRepository<SRVisit> SRVisitRepository, IBaseRepository<SRVisitDetail> SRVisitDetailRepository,
            IBaseRepository<SRVProductDetail> SRVProductDetailRepository, IBaseRepository<Employee> EmployeeRepository,
            IBaseRepository<Godown> GodownRepository,
             IBaseRepository<POrderDetail> POrderDetailRepository, IBaseRepository<Category> CategoryRepository, IBaseRepository<Company> CompanyRepository,
            IBaseRepository<POrder> POrderRepository,
        IBaseRepository<SOrder> SOrderRepository,
        IBaseRepository<SOrderDetail> SOrderDetailRepository,
        IBaseRepository<CreditSale> CreditSaleRepository,
        IBaseRepository<CreditSaleDetails> CreditSaleDetailsRepository,
             IBaseRepository<ProductUnitType> ProductUnitTypeRepository,
            IBaseRepository<Size> SizeRepository,
                 IBaseRepository<ROrderDetail> rOrderDetail,
             IBaseRepository<ROrder> RorderRepository

            )
        {
            _stockRepository = stockRepository;
            _stockDetailRepository = stockDetailRepository;
            _productRepository = productRepository;
            _ColorRepository = colorRepository;
            _unitOfWork = unitOfWork;
            _SupplierRepository = suppRepository;
            _PriceProtectionRepository = priceProtectionRepository;
            _stockRepo = stockRepo;
            _SRVisitRepository = SRVisitRepository;
            _SRVisitDetailRepository = SRVisitDetailRepository;
            _SRVProductDetailRepository = SRVProductDetailRepository;
            _EmployeeRepository = EmployeeRepository;
            _GodownRepository = GodownRepository;
            _POrderRepository = POrderRepository;
            _POrderDetailRepository = POrderDetailRepository;
            _CategoryRepository = CategoryRepository;
            _CompanyRepository = CompanyRepository;
            _SOrderRepository = SOrderRepository;
            _SOrderDetailRepository = SOrderDetailRepository;
            _CreditSaleRepository = CreditSaleRepository;
            _CreditSaleDetailsRepository = CreditSaleDetailsRepository;
            _ProductUnitTypeRepository = ProductUnitTypeRepository;
            _SizeRepository = SizeRepository;
            _RorderRepository = RorderRepository;
            _ROrderDetailRepository = rOrderDetail;
        }

        public void AddStock(Stock Stock)
        {
            _stockRepository.Add(Stock);
        }

        public void SaveStock()
        {
            _unitOfWork.Commit();
        }

        public IEnumerable<Stock> GetAllStock()
        {
            return _stockRepository.All.ToList();
        }

        public Stock GetStockById(int id)
        {
            return _stockRepository.FindBy(x => x.StockID == id).First();
        }
        public Stock GetStockByProductIdandGodownID(int ProductID, int GodownID)
        {
            return _stockRepository.FindBy(x => x.ProductID == ProductID && x.GodownID == GodownID).FirstOrDefault();
        }

        public Stock GetStockByProductIdandColorIDandGodownID(int ProductID, int GodownID, int ColorID)
        {
            return _stockRepository.FindBy(x => x.ProductID == ProductID && x.GodownID == GodownID && x.ColorID == ColorID).FirstOrDefault();
        }
        public Stock GetStockByProductId(int id)
        {
            return _stockRepository.FindBy(x => x.ProductID == id).First();
        }
        public async Task<IEnumerable<Tuple<int, string, string, string, decimal,
            decimal, decimal, Tuple<string, int, int, decimal, decimal, decimal, decimal, Tuple<string, string, string, string, string, string>>>>> GetAllStockAsync()
        {
            return await _stockRepository.GetAllStockAsync(_productRepository, _ColorRepository, _stockDetailRepository, _GodownRepository);
        }

        public async Task<IEnumerable<Tuple<int, string, string, string,
        string, string, string, Tuple<string>>>> GetAllStockDetailAsync()
        {
            return await _stockRepository.GetAllStockDetailAsync(_stockDetailRepository, _productRepository, _ColorRepository, _GodownRepository);
        }


        public IEnumerable<Tuple<int, string, string, string, string, decimal, decimal, Tuple<decimal, string, decimal, string, string>>>
          GetforStockReport(string userName, int concernID, int reportType, int CompanyID, int CategoryID, int ProductID, int GodownID, int StockType)
        {//bd
            return _stockRepository.GetforReport(_productRepository, _ColorRepository, _stockDetailRepository, _GodownRepository, 
               userName, concernID, reportType, CompanyID, CategoryID, ProductID, GodownID, StockType);
        }

        public IEnumerable<ProductWisePurchaseModel> GetforStockReportNew(string userName, int concernID, int reportType, int CompanyID, int CategoryID, int ProductID, int GodownID, EnumProductStockType ProductStockType)
        {
            return _stockRepository.GetforReportNew(_productRepository, _ColorRepository, _stockDetailRepository, _GodownRepository, _ProductUnitTypeRepository, _SizeRepository, userName, concernID, reportType, CompanyID, CategoryID, ProductID, GodownID, ProductStockType);
        }


        public IEnumerable<Tuple<string, string, decimal, decimal, decimal, decimal, DateTime>> GetPriceProtectionReport(string userName, int concernID, DateTime dFDate, DateTime dTDate)
        {
            return _stockRepository.GetPriceProtectionReport(_productRepository, _ColorRepository, _SupplierRepository, _PriceProtectionRepository, userName, concernID, dFDate, dTDate);
        }

        public IEnumerable<Tuple<int, string, string>> GetStockDetailsByID(int StockId)
        {
            return _stockRepository.GetStockDetailsByID(_stockDetailRepository, StockId);
        }


        public void DeleteStock(int id)
        {
            _stockRepository.Delete(x => x.StockID == id);
        }
        public IEnumerable<DailyStockVSSalesSummaryReportModel> DailyStockVSSalesSummary(DateTime fromDate, DateTime toDate, int concernID, int ProductID)
        {
            return _stockRepo.DailyStockVSSalesSummary(fromDate, toDate, concernID, ProductID);
        }
        public bool IsIMEIAvailableForSRVisit(int ProductID, int ColorID, string IMEI)
        {
            return _stockRepository.CheckStockIMEIForSRVisit(_stockDetailRepository, ProductID, ColorID, IMEI);
        }
        public string GetStockProductsHistory(int StockID)
        {
            return _stockRepository.GetStockProductsHistory(_stockDetailRepository, _SRVisitRepository, _SRVisitDetailRepository, _SRVProductDetailRepository, _EmployeeRepository, StockID);
        }

        //public List<StockLedger> GetStockLedgerReport(int reportType, int CompanyID, int CategoryID, int ProductID, DateTime dFDate, DateTime dTDate)
        //{
        //    return _stockRepository.GetStockLedger(_stockDetailRepository, _POrderRepository,
        //        _POrderDetailRepository, _productRepository, _CategoryRepository, _CompanyRepository, _ColorRepository, _SOrderRepository, _SOrderDetailRepository, _CreditSaleRepository, _CreditSaleDetailsRepository, _RorderRepository, _ROrderDetailRepository, reportType, CompanyID, CategoryID, ProductID, dFDate, dTDate);
        //}

        public List<StockLedger> GetStockLedgerReport(int reportType, string CompanyName, string CategoryName, string ProductName, DateTime dFDate, DateTime dTDate, int ConcernID)
        {
            return _stockRepository.GetStockLedger(_stockDetailRepository, _POrderRepository,
                _POrderDetailRepository, _productRepository, _CategoryRepository, _CompanyRepository, _ColorRepository, _SOrderRepository, _SOrderDetailRepository, _RorderRepository,
                _ROrderDetailRepository, _SizeRepository, reportType, CompanyName, CategoryName, ProductName, dFDate, dTDate, ConcernID);
        }
        public IQueryable<ProductDetailsModel> GetStockDetails()
        {
            return _stockRepository.GetStockDetails(_stockDetailRepository, _productRepository, _CompanyRepository, _CategoryRepository, _GodownRepository, _ColorRepository);
        }


        public List<ProductDetailsModel> GetStockProductsBySupplier(int SupplierID)
        {
            return _stockRepository.GetStockProductsBySupplier(_stockDetailRepository, _productRepository, _ColorRepository, _SupplierRepository, _POrderRepository,
                _POrderDetailRepository, _CategoryRepository, _CompanyRepository, _ProductUnitTypeRepository, SupplierID , _SizeRepository);
        }

        public bool UpdateProductSalePrice(int productId, int unitTypeId, decimal saleRate)
        {
            ProductUnitType productUnit = _ProductUnitTypeRepository.FindBy(d => d.ProUnitTypeID == unitTypeId).FirstOrDefault();
            List<StockDetail> stockDetails = _stockDetailRepository.All.Where(d => d.ProductID == productId && d.Status == 1).ToList();

            foreach (var item in stockDetails)
            {
                item.SRate = saleRate / productUnit.ConvertValue;
                _stockDetailRepository.Update(item);
            }
            try
            {
                SaveStock();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public List<ProductDetailsModel> GetSupplierStockDetails(int SupplierID, int ProductID, int ColorID, int GodownID)
        {
            return _stockRepository.GetSupplierStockDetails(_stockDetailRepository, _POrderRepository,
                _POrderDetailRepository, _productRepository, SupplierID, ProductID, ColorID, GodownID);
        }
        public List<ProductDetailsModel> GetSupplierDamageStockDetails(int SupplierID, int ProductID, int ColorID, int GodownID)
        {
            return _stockRepository.GetSupplierDamageStockDetails(_stockDetailRepository, _POrderRepository,
                _POrderDetailRepository, _productRepository, SupplierID, ProductID, ColorID, GodownID);
        }

        public List<ProductDetailsModel> GetDamageStockProductsBySupplier(int SupplierID)
        {
            return _stockRepository.GetDamageStockProductsBySupplier(_stockDetailRepository, _productRepository, _ColorRepository, _SupplierRepository, _POrderRepository,
                _POrderDetailRepository, _CategoryRepository, _CompanyRepository, _ProductUnitTypeRepository, SupplierID);
        }

        public IQueryable<Stock> GetAll()
        {
            return _stockRepository.All;
        }

    }
}
