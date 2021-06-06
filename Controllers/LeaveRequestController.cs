using AutoMapper;
using leave_management.Contracts;
using leave_management.Data;
using leave_management.Models;
using leave_management.Repository;
using leave_management.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Controllers
{
    [Authorize]
    public class LeaveRequestController : Controller
    {
        private readonly ILeaveRequestRepository _leaveRequestRepo;
        private readonly ILeaveTypeRepository _leaveTypeRepo;
        private readonly ILeaveAllocationRepository _leaveAllocationRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;
        private readonly UserManager<Employee> _userManager;

        public LeaveRequestController (ILeaveRequestRepository leaveRequestRepo, ILeaveTypeRepository leaveTypeRepo, ILeaveAllocationRepository leaveAllocationRepo, IUnitOfWork unitOfWork, IEmailSender emailSender, IMapper mapper, UserManager<Employee> userManager)
        {
            _leaveRequestRepo = leaveRequestRepo;
            _leaveTypeRepo = leaveTypeRepo;
            _leaveAllocationRepo = leaveAllocationRepo;
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _mapper = mapper;
            _userManager = userManager;
        }

        [Authorize(Roles = "Administrator")] //only role administaror can access this method/view
        // GET: LeaveRequestController
        public async Task<ActionResult> Index()
        {
            //var leaveRequests = await _leaveRequestRepo.FindAll();
            var leaveRequests = await _unitOfWork.LeaveRequests.FindAll(
                    includes: new List<string> { "RequestingEmployee", "LeaveType" } // include parameter
                );
            var leaveRequestsModel = _mapper.Map<List<LeaveRequestVM>>(leaveRequests);
            var model = new AdminLeaveRequestViewVM
            {
                TotalRequests = leaveRequestsModel.Count,
                ApprovedRequests = leaveRequestsModel.Where(q => q.Approved == true).Count(),
                PendingRequests = leaveRequestsModel.Count(q => q.Approved == null),
                RejectedRequests = leaveRequestsModel.Count(q => q.Approved == false), // like query =>  EF.Functions.Like(q.ApprovedById, "[aeiou]%")
                LeaveRequest = leaveRequestsModel
            };

            return View(model);
        }

        public async Task<ActionResult> MyLeave()
        {
            var employee = await _userManager.GetUserAsync(User);
            //var employeeAllocations = await _leaveAllocationRepo.GetLeaveAllocationsByEmployee(employee.Id);
            var employeeAllocations = await _unitOfWork.LeaveAllocations.FindAll(
                    q => q.EmployeeId == employee.Id,
                    includes: new List<string> { "LeaveType" }
                );
            //var employeeRequests = await _leaveRequestRepo.GetLeaveRequestByEmployee(employee.Id);
            var employeeRequests = await _unitOfWork.LeaveRequests.FindAll(q => q.RequestingEmployeeId == employee.Id);

            var empAllocationModel = _mapper.Map<List<LeaveAllocationVM>>(employeeAllocations);
            var empRequestModel = _mapper.Map<List<LeaveRequestVM>>(employeeRequests);

            var model = new EmployeeLeaveRequestVM
            {
                LeaveAllocations = empAllocationModel,
                LeaveRequests = empRequestModel
            };

            return View(model);
        }

        public async Task<ActionResult> CancelRequest(int id)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var employee = await _userManager.GetUserAsync(User);
            //var empLeaveRequest = await _leaveRequestRepo.FindById(id);
            var empLeaveRequest = await _unitOfWork.LeaveRequests.Find(q => q.Id == id);
            int daysRequested = (int)(empLeaveRequest.EndDate - empLeaveRequest.StartDate).TotalDays;
            //var isSucceedDel = await _leaveRequestRepo.Delete(empLeaveRequest); // delete leave request
            //if (!isSucceedDel)
            //{
            //    return BadRequest();
            //}
            _unitOfWork.LeaveRequests.Delete(empLeaveRequest);
            await _unitOfWork.Save();

            return RedirectToAction(nameof(MyLeave));
        }

        // GET: LeaveRequestController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            //var leaveRequest = await _leaveRequestRepo.FindById(id);
            var leaveRequest = await _unitOfWork.LeaveRequests.Find(
                    q => q.Id == id,
                    includes: new List<string> { "ApprovedBy", "RequestingEmployee", "LeaveType" }
                   );
            var model = _mapper.Map<LeaveRequestVM>(leaveRequest);
            return View(model);
        }

        public async Task<ActionResult> ApproveRequest(int id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }
                
                var employee = await _userManager.GetUserAsync(User);
                //var leaveRequest = await _leaveRequestRepo.FindById(id);
                var leaveRequest = await _unitOfWork.LeaveRequests.Find(q => q.Id == id);
                leaveRequest.Approved = true;
                leaveRequest.ApprovedById = employee.Id;
                leaveRequest.DateActioned = DateTime.Now;
                int daysRequested = (int)(leaveRequest.EndDate - leaveRequest.StartDate).TotalDays;

                //var leaveAllocation = await _leaveAllocationRepo.GetLeaveAllocationsByEmployeeAndType(leaveRequest.RequestingEmployeeId, leaveRequest.LeaveTypeId);
                var leaveAllocation = await _unitOfWork.LeaveAllocations.Find(q =>
                    q.EmployeeId == leaveRequest.RequestingEmployeeId &&
                    q.LeaveTypeId == leaveRequest.LeaveTypeId &&
                    q.Period == DateTime.Now.Year
                    );
                leaveAllocation.NumberOfDays -= daysRequested;

                //var isSucceddLeaveAllocation = await _leaveAllocationRepo.Update(leaveAllocation);
                //var isSucceed = await _leaveRequestRepo.Update(leaveRequest);
                //if (!isSucceed && !isSucceddLeaveAllocation)
                //{
                //    return BadRequest();
                //}

                _unitOfWork.LeaveAllocations.Update(leaveAllocation);
                _unitOfWork.LeaveRequests.Update(leaveRequest);
                await _unitOfWork.Save();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Something went wrong approve request");
                return View();
            }
            
        }

        public async Task<ActionResult> RejectRequest(int id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }

                var employee = await _userManager.GetUserAsync(User);
                //var leaveRequest = await _leaveRequestRepo.FindById(id);
                var leaveRequest = await _unitOfWork.LeaveRequests.Find(q => q.Id == id);
                leaveRequest.Approved = false;
                leaveRequest.ApprovedById = employee.Id;
                leaveRequest.DateActioned = DateTime.Now;

                //var isSucceed = await _leaveRequestRepo.Update(leaveRequest);
                //if (!isSucceed)
                //{
                //    return BadRequest();
                //}

                _unitOfWork.LeaveRequests.Update(leaveRequest);
                await _unitOfWork.Save();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Something went wrong reject request");
                return View();
            }
            
        }

        // GET: LeaveRequestController/Create
        public async Task<ActionResult> Create()
        {
            //var leaveTypes = await _leaveTypeRepo.FindAll();
            var leaveTypes = await _unitOfWork.LeaveTypes.FindAll();
            var leaveTypeItems = leaveTypes.Select(q => new SelectListItem { 
                Text = q.Name,
                Value = q.Id.ToString()
            }); //set data to dropdown attribute
            var model = new CreateLeaveRequestVM
            {
                LeaveTypes = leaveTypeItems                
            };

            return View(model);
        }

        // POST: LeaveRequestController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateLeaveRequestVM model)
        {

            try
            {
                var startDate = Convert.ToDateTime(model.StartDate);
                var endDate = Convert.ToDateTime(model.EndDate);
                //var leaveTypes = await _leaveTypeRepo.FindAll();
                var leaveTypes = await _unitOfWork.LeaveTypes.FindAll();
                var leaveTypeItems = leaveTypes.Select(q => new SelectListItem
                {
                    Text = q.Name,
                    Value = q.Id.ToString()
                }); //set data to dropdown attribute
                model.LeaveTypes = leaveTypeItems;

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if (DateTime.Compare(startDate, endDate) > 1)
                {
                    ModelState.AddModelError("", "Start date cannot be further in the future than the End Date");
                    return View(model);
                }

                var employee = await _userManager.GetUserAsync(User);
                //var allocation = await _leaveAllocationRepo.GetLeaveAllocationsByEmployeeAndType(employee.Id, model.LeaveTypeId);
                var allocation = await _unitOfWork.LeaveAllocations.Find(q => 
                    q.EmployeeId == employee.Id &&
                    q.LeaveTypeId == model.LeaveTypeId &&
                    q.Period == DateTime.Now.Year
                    );
                int daysRequested = (int)(endDate - startDate).TotalDays;
                if(daysRequested > allocation.NumberOfDays)
                {
                    ModelState.AddModelError("", "You do not have sufficient days of this request");
                    return View(model);
                }

                var leaveRequestModel = new LeaveRequestVM
                {
                    RequestingEmployeeId = employee.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    Approved = null,
                    DateRequested = DateTime.Now,
                    DateActioned = DateTime.Now,
                    LeaveTypeId = model.LeaveTypeId
                };
                var leaveRequest = _mapper.Map<LeaveRequest>(leaveRequestModel);
                //var isSucceed = await _leaveRequestRepo.Create(leaveRequest);
                //if (!isSucceed)
                //{
                //    ModelState.AddModelError("", "Something went wrong when submitting your record");
                //    return View(model);
                //}
                await _unitOfWork.LeaveRequests.Create(leaveRequest);
                await _unitOfWork.Save();

                //sent email
                await _emailSender.SendEmailAsync("fakhrul131096@gmail.com", "Confirm your email",
                        $"<h1>New leave request.</h1> <br/> Please review leave request.");

                return RedirectToAction(nameof(MyLeave));
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Something went wrong");
                return View(model);
            }
        }

        // GET: LeaveRequestController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LeaveRequestController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LeaveRequestController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveRequestController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        protected override void Dispose(bool disposing)
        {
            _unitOfWork.Dispose();
            base.Dispose(disposing);
        }
    }
}
