using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.Text;
using System.IO;
using System.Globalization;

namespace DirRX.StudyIntegration.Server
{
  public class ModuleJobs
  {

    /// <summary>
    /// Фоновый процесс, загружающий пользователей в систему из csv файла системы RUBI.
    /// </summary>
    public virtual void GetEmployeesFromCsv()
    {     
      try
      {
      // Читаем csv файл с интеграционными данными и создаём структуру в памяти.
      var filePath = DirRX.StudyIntegration.Resources.IntegrationWithRubiFilePath;
      var employeeInfoList = new List<Structures.Module.IEmployeeInfo>();
      
      using( var fs = new FileStream(filePath, FileMode.Open))
      {
        try
          {
            var package = new System.Text.StringBuilder();
            using (var sr = new StreamReader(fs, Encoding.UTF8))
            {
              var line = string.Empty;
              
              // Пропустить первую строчку.
              sr.ReadLine();
              
              while ((line = sr.ReadLine()) != null)
              {
                var employeeRubyInfo = line.Split(Constants.Module.RubiCsvSplitSymbol);             
                var employeeInfo = Structures.Module.EmployeeInfo.Create();
                 
                employeeInfo.WWID = employeeRubyInfo[Constants.Module.RubiWWIDIndex];
                employeeInfo.Login = employeeRubyInfo[Constants.Module.RubiLoginIndex];
                employeeInfo.BusinessEmail = employeeRubyInfo[Constants.Module.RubiBusinessEmailIndex];
                employeeInfo.Status = employeeRubyInfo[Constants.Module.RubiStatusIndex];
                
                employeeInfo.StartDate = DateTime.ParseExact(employeeRubyInfo[Constants.Module.RubiStartDateIndex],
                  "dd.mm.yyyy", CultureInfo.InvariantCulture);
                
                employeeInfo.EndDate = String.IsNullOrEmpty(employeeRubyInfo[Constants.Module.RubiEndDateIndex]) ?
                  (DateTime?) null :
                  DateTime.ParseExact(employeeRubyInfo[Constants.Module.RubiEndDateIndex], 
                    "dd.mm.yyyy", CultureInfo.InvariantCulture);
                
                employeeInfo.LastName = employeeRubyInfo[Constants.Module.RubiLastNameIndex];
                employeeInfo.FirstName = employeeRubyInfo[Constants.Module.RubiFirstNameIndex];
                employeeInfo.MiddleName = employeeRubyInfo[Constants.Module.RubiMiddleNameIndex];
                employeeInfo.PositionName = employeeRubyInfo[Constants.Module.RubiPositionNameIndex];
                employeeInfo.Divcode = employeeRubyInfo[Constants.Module.RubiDivcodeIndex];
                employeeInfo.ManagerWWID = employeeRubyInfo[Constants.Module.RubiManagerWWIDIndex];
                 
                employeeInfoList.Add(employeeInfo);
               }
             }
          }
          catch(Exception ex)
          {
            Logger.DebugFormat("Интеграция с Rubi. Ошибка при чтении csv файла: {0}", ex.Message);
          }
        }
        
        // Сортируем по WWID руководителя и WWID, чтобы вначале сохранять/изменять сотрудников без руководителя.
        employeeInfoList = employeeInfoList
          .OrderBy(e => e.ManagerWWID)
          .ThenBy(e => e.WWID)
          .ToList();
        
        var notSuccessList = new List<Structures.Module.IEmployeeInfo>();
        
        // Перебираем записи структуры.
        foreach (var employeeInfo in employeeInfoList)
        {
          try
          {
            var employee = DirRX.IntegrationRubi.Employees.GetAll(ei => ei.WWIDDirRX.Equals(employeeInfo.WWID)).FirstOrDefault();
            var person = Sungero.Parties.People.Null;
            var login = Sungero.CoreEntities.Logins.Null;
            var department = Sungero.Company.Departments.GetAll(d => d.Code.Equals(employeeInfo.Divcode)).FirstOrDefault();
            var jobTitle = Sungero.Company.JobTitles.GetAll(jt => jt.Name.Equals(employeeInfo.PositionName)).FirstOrDefault();
            
            // Получить руководителя из уже созданных сотрудников в базе.
            var manager = DirRX.IntegrationRubi.Employees.GetAll(e => e.WWIDDirRX.Equals(employeeInfo.ManagerWWID)).FirstOrDefault();
            
            // Если сотрудника нет в БД, то необходимо создать сотрудника, учётную запись и персону.
            if (employee == null)
            {
              employee = DirRX.IntegrationRubi.Employees.Create();
              person = Sungero.Parties.People.Create();
              login = Sungero.CoreEntities.Logins.Create();    
            }
            // Если сотрудник уже создан в БД, то необходимо получить его учётную запись и персону.
            else
            {
              person = Sungero.Parties.People.Get(employee.Person.Id);
              login = Sungero.CoreEntities.Logins.Get(employee.Login.Id);
            }
            
            // Если найдено подразделение у которого Код = Divcode.
            if (department != null)
            {
              employee.State.Properties.Department.IsRequired = true;                
              
              if (!employee.Department.Equals(department))
                employee.Department = department;
            }
            else
            {
              employee.State.Properties.Department.IsRequired = false;
              if (employee.Department != null)
                employee.Department = Sungero.Company.Departments.Null;
            }
            
            // Если найдена должность, у которой Наименование = PositionName.
            if (jobTitle != null)
            {
              employee.State.Properties.JobTitle.IsRequired = true;
              
              if (!employee.JobTitle.Equals(jobTitle))
                employee.JobTitle = jobTitle;
            }
            else
            {
              employee.State.Properties.JobTitle.IsRequired = false;
              
              if (employee.JobTitle != null)
                employee.JobTitle = Sungero.Company.JobTitles.Null;
            }
            
            // Если Status не равен «terminated», то заполняется значением «Действующая». 
            // В противном случае заполняется значением «Закрытая».
            if (employeeInfo.Status == Constants.Module.EmployeeTerminatedStatus)
            {
              if (employee.Status != DirRX.IntegrationRubi.Employee.Status.Closed)
                employee.Status = DirRX.IntegrationRubi.Employee.Status.Closed;
            }
            else
            {
              if (employee.Status != DirRX.IntegrationRubi.Employee.Status.Active)
                employee.Status = DirRX.IntegrationRubi.Employee.Status.Active;
            }
            
            // Если найден руководитель сотрудника, то заполняется поле Руководитель.
            if(manager != null)
            {
              if (!manager.Equals(employee.MangerDirRX))
                employee.MangerDirRX = manager;
            }
            else
            {
              if (employee.MangerDirRX != null)
                manager = DirRX.IntegrationRubi.Employees.Null;
            }
            
            if (person.FirstName != employeeInfo.FirstName)
              person.FirstName = employeeInfo.FirstName;
            
            if (person.LastName != employeeInfo.LastName)
              person.LastName = employeeInfo.LastName;
            
            if (person.MiddleName !=employeeInfo.MiddleName)
              person.MiddleName = employeeInfo.MiddleName;
            
            if (login.LoginName != employeeInfo.Login)
              login.LoginName = employeeInfo.Login;
            
            if (person.State.IsChanged)
              employee.Person = person;
            
            if (login.State.IsChanged)
              employee.Login = login;
            
            if (employee.Email != employeeInfo.BusinessEmail)
              employee.Email = employeeInfo.BusinessEmail;
            
            if (employee.StartDateDirRX.Value.Date != employeeInfo.StartDate.Value.Date)
              employee.StartDateDirRX = employeeInfo.StartDate;
            
            if (employee.EndDateDirRX.Value.Date != employeeInfo.EndDate.Value.Date)
              employee.EndDateDirRX = employeeInfo.EndDate;
            
            if (employee.WWIDDirRX != employeeInfo.WWID)
              employee.WWIDDirRX = employeeInfo.WWID;
            
            if (employee.NeedNotifyExpiredAssignments != false)
              employee.NeedNotifyExpiredAssignments = false;
            
            if (employee.NeedNotifyNewAssignments != false)
              employee.NeedNotifyNewAssignments = false;
     
            // Сохранение в БД.
            if (person.State.IsChanged)
              person.Save();
            
            if (login.State.IsChanged)
              login.Save();
            
            if (employee.State.IsChanged)
              employee.Save();
          }       
          catch(Exception ex)
          {
            Logger.DebugFormat("Интеграция с Rubi. Ошибка при создании/обновлении сотрудника WWID = {0}: {1}",
              employeeInfo.WWID, ex.Message);
            notSuccessList.Add(employeeInfo);
          }
        }
        
        if (notSuccessList.Count() != 0)
        {
          Logger.DebugFormat("Интеграция с Rubi. Количество необработанных записей {0} из {1}",
            notSuccessList.Count, employeeInfoList.Count());
        }
      }
      catch (Exception ex)
      {
        Logger.DebugFormat("Интеграция с Rubi. Ошибка при выполнении фонового процесса: {0}", ex.Message);
      }
    }
  }
}
