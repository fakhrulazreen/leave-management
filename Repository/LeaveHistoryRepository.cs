﻿using leave_management.Contracts;
using leave_management.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Repository
{
    public class LeaveHistoryRepository : ILeaveHistoryRepository
    {
        private readonly ApplicationDbContext _db;

        public LeaveHistoryRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public bool Create(LeaveHistory entity)
        {
            _db.LeaveHistories.Add(entity);
            return Save();
        }

        public bool Delete(LeaveHistory entity)
        {
            _db.LeaveHistories.Remove(entity);
            return Save();
        }

        public ICollection<LeaveHistory> FindAll()
        {
            var leaveHistorys = _db.LeaveHistories.ToList();
            return leaveHistorys;
        }

        public LeaveHistory FindById(int id)
        {
            var leaveHistory = _db.LeaveHistories.Find(id);
            return leaveHistory;
        }

        public bool isExists(int id)
        {
            var exists = _db.LeaveHistories.Any(q => q.Id == id); // any() check if empty record is empty or not
            return exists;
        }

        public bool Save()
        {
            var changes = _db.SaveChanges();
            return changes > 0; //bcs anything update/delete/add will have one or more record, if less than one then error on the operation
        }

        public bool Update(LeaveHistory entity)
        {
            _db.LeaveHistories.Update(entity);

            return Save();
        }
    }
}
