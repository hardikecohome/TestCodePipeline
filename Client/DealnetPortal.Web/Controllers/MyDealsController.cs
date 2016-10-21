using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.ServiceAgent;
using System.IO;

namespace DealnetPortal.Web.Controllers
{
    [AuthFromContext]
    public class MyDealsController : Controller
    {
        private readonly IContractServiceAgent _contractServiceAgent;
        private readonly IContractManager _contractManager;
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;

        public MyDealsController(IContractServiceAgent contractServiceAgent, IDictionaryServiceAgent dictionaryServiceAgent,
            IContractManager contractManager)
        {
            _contractServiceAgent = contractServiceAgent;
            _dictionaryServiceAgent = dictionaryServiceAgent;
            _contractManager = contractManager;
        }
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> ContractEdit(int id)
        {
            ViewBag.EquipmentTypes = (await _dictionaryServiceAgent.GetEquipmentTypes()).Item1;
            return View(await _contractManager.GetContractEditAsync(id));
        }

        [HttpPost]
        public  ActionResult UploadDocument(HttpPostedFileBase files )
        {           
            if (files != null)
             //   foreach(var file in files)
            {
                string fileName = Guid.NewGuid().ToString();
                string extension = Path.GetExtension(files.FileName);
                fileName += extension;
                files.SaveAs(Server.MapPath(@"/App_Data/Upload/" + fileName));
                return Json("File was saved", JsonRequestBehavior.DenyGet);
            }

            return Json("occurred error", JsonRequestBehavior.DenyGet);
        }
    }
}