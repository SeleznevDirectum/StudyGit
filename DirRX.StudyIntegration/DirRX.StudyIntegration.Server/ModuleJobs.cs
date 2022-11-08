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
        #region Чтение csv файла с интеграционными данными и создание структуры employeeInfoList в памяти.
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
                
                #region Преобразование даты.
                var startDate = default(DateTime);
                
                if (!DateTime.TryParseExact(employeeRubyInfo[Constants.Module.RubiStartDateIndex],
                                            Constants.Module.DateTimeParseFormat,
                                            CultureInfo.InvariantCulture,
                                            System.Globalization.DateTimeStyles.None,
                                            out startDate))
                {
                  throw new Exception("Не удалось преобразовать дату для поля StartDate.");
                }
                
                employeeInfo.StartDate = startDate;
                
                var endDate = default(DateTime);
                
                if (String.IsNullOrEmpty(employeeRubyInfo[Constants.Module.RubiEndDateIndex]))
                {
                  employeeInfo.EndDate = null;
                }
                else
                {
                  if (!DateTime.TryParseExact(employeeRubyInfo[Constants.Module.RubiEndDateIndex],
                                              Constants.Module.DateTimeParseFormat,
                                              CultureInfo.InvariantCulture,
                                              System.Globalization.DateTimeStyles.None,
                                              out endDate))
                  {
                    throw new Exception("Не удалось преобразовать дату для поля EndDate.");
                  }
                  
                  employeeInfo.EndDate = endDate;
                }
                #endregion
                
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
            Logger.ErrorFormat("Интеграция с Rubi. Ошибка при чтении csv файла: {0}", ex.Message);
          }
        }
        #endregion
        
        // Сортировка по WWID руководителя, для сохранения/изменения в первую очередь сотрудников без руководителя.
        employeeInfoList = employeeInfoList
          .OrderBy(e => e.ManagerWWID)
          .ThenBy(e => e.WWID)
          .ToList();
        
        // Список необработанных записей.
        var notSuccessList = new List<Structures.Module.IEmployeeInfo>();
        
        #region Создание/обновление сотрудников в DirectumRX.
        foreach (var employeeInfo in employeeInfoList)
        {
          try
          {
            #region Создание/получение сотрудника.
            var employee = DirRX.IntegrationRubi.Employees.GetAll(ei => ei.WWIDDirRX.Equals(employeeInfo.WWID)).FirstOrDefault();
            var person = Sungero.Parties.People.Null;
            var login = Sungero.CoreEntities.Logins.Null;
            
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
              
              if (login == null)
                login = Sungero.CoreEntities.Logins.Create();
            }
            #endregion
            
            #region Изменение подразделения сотрудника.
            if (string.IsNullOrEmpty(employeeInfo.Divcode))
            {
              if (employee.Department != null)
                employee.Department = Sungero.Company.Departments.Null;
            }
            else
            {
              var department = Sungero.Company.Departments.GetAll(d => d.Code.Equals(employeeInfo.Divcode)).FirstOrDefault();
              
              // Если найдено подразделение у которого Код = Divcode.
              if (department != null)
              {
                // Если найденное подразделение не совпадает со старым, то изменяем подразделение.
                if (!employee.Department.Equals(department))
                  employee.Department = department;
              }
            }
            #endregion
            
            #region Изменение должности сотрудника.
            if (string.IsNullOrEmpty(employeeInfo.PositionName))
            {
              if (employee.JobTitle != null)
                employee.JobTitle = employee.JobTitle = Sungero.Company.JobTitles.Null;
            }
            else
            {
              var jobTitle = Sungero.Company.JobTitles.GetAll(jt => jt.Name.Equals(employeeInfo.PositionName)).FirstOrDefault();
              
              // Если найдена должность, у которой Наименование = PositionName.
              if (jobTitle != null)
              {
                // Если найденная должность не совпадает со старой, то меняем на новую должность.
                if (!employee.JobTitle.Equals(jobTitle))
                  employee.JobTitle = jobTitle;
              }
            }
            #endregion
            
            #region Изменение состояния сотрудника.
            
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
            #endregion
            
            #region Изменение руководителя сотрудника.
            
            // Получить руководителя из уже созданных сотрудников в базе.
            if (string.IsNullOrEmpty(employeeInfo.ManagerWWID))
            {
              if (employee.MangerDirRX != null)
                employee.MangerDirRX = DirRX.IntegrationRubi.Employees.Null;
            }
            else
            {
              var manager = DirRX.IntegrationRubi.Employees.GetAll(e => e.WWIDDirRX.Equals(employeeInfo.ManagerWWID)).FirstOrDefault();
              
              // Если найден руководитель сотрудника, то заполняется поле Руководитель.
              if(manager != null)
              {
                if (!DirRX.IntegrationRubi.Employees.Equals(manager, employee.MangerDirRX))
                  employee.MangerDirRX = manager;
              }
            }
            #endregion
            
            #region Изменение значений полей сотрудника.
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
            
            if (employee.StartDateDirRX != employeeInfo.StartDate)
              employee.StartDateDirRX = employeeInfo.StartDate;
            
            if (employee.EndDateDirRX != employeeInfo.EndDate)
              employee.EndDateDirRX = employeeInfo.EndDate;
            
            if (employee.WWIDDirRX != employeeInfo.WWID)
              employee.WWIDDirRX = employeeInfo.WWID;
            
            if (employee.NeedNotifyExpiredAssignments != false)
              employee.NeedNotifyExpiredAssignments = false;
            
            if (employee.NeedNotifyNewAssignments != false)
              employee.NeedNotifyNewAssignments = false;
            
            #endregion
            
            #region Выключение обязательности заполнения подразделения и должности.
            employee.State.Properties.Department.IsRequired = false;
            employee.State.Properties.JobTitle.IsRequired = false;
            #endregion
            
            #region Сохранение в БД.
            if (person.State.IsChanged)
              person.Save();
            
            if (login.State.IsChanged)
              login.Save();
            
            if (employee.State.IsChanged)
              employee.Save();
            
            #endregion
            
            #region Включение обязательности заполнения подразделения и должности.
            employee.State.Properties.Department.IsRequired = true;
            employee.State.Properties.JobTitle.IsRequired = true;
            #endregion
          }
          catch(Exception ex)
          {
            Logger.ErrorFormat("Интеграция с Rubi. Ошибка при создании/обновлении сотрудника WWID = {0}: {1}",
                               employeeInfo.WWID, ex.Message);
            
            // Добавление необработанную запись в список notSuccessList.
            notSuccessList.Add(employeeInfo);
          }
        }
        #endregion
        
        #region Логирование результатов интеграции.
        if (notSuccessList.Count() != 0)
        {
          Logger.ErrorFormat("Интеграция с Rubi. Количество необработанных записей {0} из {1}",
                             notSuccessList.Count, employeeInfoList.Count());
        }
        else
        {
          Logger.Debug("Интеграция с Rubi. Данные сотрудников успешно обновлены.");
        }
        #endregion
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("Интеграция с Rubi. Ошибка при выполнении фонового процесса: {0}", ex.Message);
      }
    }
  }
}
