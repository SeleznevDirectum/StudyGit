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
    /// 
    /// </summary>
    public virtual void GetEmployeesFromCsv()
    {     
      //Читаем csv файл с интеграционными данными и создаём структуру в памяти
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
            
            // Пропустить первую строчку
            sr.ReadLine();
            
            while ((line = sr.ReadLine()) != null)
            {
              var employeeRubyInfo = line.Split(';');             
              var employeeInfo = Structures.Module.EmployeeInfo.Create();
               
              employeeInfo.WWID = employeeRubyInfo[0];
              employeeInfo.Login = employeeRubyInfo[1];
              employeeInfo.BusinessEmail = employeeRubyInfo[2];
              employeeInfo.Status = employeeRubyInfo[3];
              employeeInfo.StartDate = employeeRubyInfo[4];
              employeeInfo.EndDate = employeeRubyInfo[5];
              employeeInfo.LastName = employeeRubyInfo[6];
              employeeInfo.FirstName = employeeRubyInfo[7];
              employeeInfo.MiddleName = employeeRubyInfo[8];
              employeeInfo.PositionName = employeeRubyInfo[9];
              employeeInfo.Divcode = employeeRubyInfo[10];
              employeeInfo.ManagerWWID = employeeRubyInfo[11];
               
              employeeInfoList.Add(employeeInfo);
             }
           }
        }
        catch(Exception ex)
        {

        }
      }
      //Сортируем по WWID руководителя и WWID, чтобы вначале сохранять/изменять сотрудников без руководителя.
      employeeInfoList = employeeInfoList
        .OrderBy(e => e.ManagerWWID)
        .ThenBy(e => e.WWID)
        .ToList();
      
      //Перебираем записи структуры
      foreach (var employeeInfo in employeeInfoList)
      {
        var employee = DirRX.IntegrationRubi.Employees.GetAll().FirstOrDefault(ei => ei.WWIDDirRX.Equals(employeeInfo.WWID));
        var person = Sungero.Parties.People.Null;
        var login = Sungero.CoreEntities.Logins.Null;
        var department = Sungero.Company.Departments.GetAll().FirstOrDefault(d => d.Code.Equals(employeeInfo.Divcode));
        var jobTitle = Sungero.Company.JobTitles.GetAll().FirstOrDefault(jt => jt.Name.Equals(employeeInfo.PositionName));                
        
        //Получаем руководителя из уже созданных сотрудников в базе.
        var manager = DirRX.IntegrationRubi.Employees.GetAll().FirstOrDefault(e => e.WWIDDirRX.Equals(employeeInfo.ManagerWWID));
        
        //Если сотрудника нет в БД, то создаём сотрудника, учётную запись и персону.
        if (employee == null)
        {
          employee = DirRX.IntegrationRubi.Employees.Create();
          person = Sungero.Parties.People.Create();
          login = Sungero.CoreEntities.Logins.Create();    
        }
        // Если сотрудник уже создан в БД, то получаем его учётную запись и персону.
        else
        {
          person = Sungero.Parties.People.Get(employee.Person.Id);
          login = Sungero.CoreEntities.Logins.Get(employee.Login.Id);
        }
        
        //Если найдено подразделение у которого Код = Divcode.
        if (department != null)
          employee.Department = department;
        else
          employee.State.Properties.Department.IsRequired = false;
       
        //Если найдена должность у которой Наименование = PositionName.
        if (jobTitle != null)
          employee.JobTitle = jobTitle;
        else
          employee.State.Properties.JobTitle.IsRequired = false;
        
        //Заполнение даты приёма.
        if(!String.IsNullOrEmpty(employeeInfo.StartDate))
          employee.StartDateDirRX = DateTime.ParseExact(employeeInfo.StartDate, "dd.mm.yyyy", CultureInfo.InvariantCulture);
        
        //Заполнение даты увольнения.
        if(!String.IsNullOrEmpty(employeeInfo.EndDate))
          employee.EndlDateDirRX = DateTime.ParseExact(employeeInfo.EndDate, "dd.mm.yyyy", CultureInfo.InvariantCulture);

        //Если Status не равен «terminated», то заполняется значением «Действующая». В противном случае заполняется значением «Закрытая»..
        if (employeeInfo.Status.Equals("terminated"))
          employee.Status = DirRX.IntegrationRubi.Employee.Status.Closed;
        else
          employee.Status = DirRX.IntegrationRubi.Employee.Status.Active;
        
        if(manager != null)
          employee.MangerDirRX = manager;
        
        person.FirstName = employeeInfo.FirstName;
        person.LastName = employeeInfo.LastName;
        person.MiddleName = employeeInfo.MiddleName;       
        login.LoginName = employeeInfo.Login;       
        employee.Person = person;
        employee.Login = login;
        employee.Email = employeeInfo.BusinessEmail;
        employee.WWIDDirRX = employeeInfo.WWID;
        employee.NeedNotifyExpiredAssignments = false;
        employee.NeedNotifyNewAssignments = false;
                           
        //Сохранение в БД
        person.Save();
        login.Save();
        employee.Save();
      }
    }
  }
}
