using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.Text;
using System.IO;


namespace DirRX.StudyIntegration.Server
{
  public class ModuleJobs
  {

    /// <summary>
    /// Фоновый процесс, загружающий пользователей в систему из csv файла системы RUBI.
    /// </summary>
    public virtual void GetEmployeesFromCsv()
    {
      Logger.Debug("GetEmployeesFromCsv. Старт процесса.");
      try
      {
        #region Чтение csv файла с интеграционными данными и создание структуры employeeInfoList в памяти.
        var filePath = DirRX.StudyIntegration.Resources.IntegrationWithRubiFilePath;
        var employeeInfoList = new List<Structures.Module.IEmployeeInfo>();
        var linesNumber = 0;
        
        if (!File.Exists(filePath))
        {
          Logger.ErrorFormat("GetEmployeesFromCsv. Файла не существует: {0}.", filePath);
          return;
        }
        
        Logger.DebugFormat("GetEmployeesFromCsv. Начало чтения данных из csv файла: {0}.", filePath);
        using( var fs = new FileStream(filePath, FileMode.Open))
        {
          var package = new System.Text.StringBuilder();
          using (var sr = new StreamReader(fs, Encoding.UTF8))
          {
            var line = string.Empty;
            // Пропустить первую строчку.
            sr.ReadLine();
            
            while ((line = sr.ReadLine()) != null)
            {
              try
              {
                linesNumber ++;
                Logger.DebugFormat("GetEmployeesFromCsv. Номер строки: {0}. Обработка строки.", linesNumber);
                var employeeRubyInfo = line.Split(Constants.Module.RubiCsvSplitSymbol);
                var employeeInfo = Structures.Module.EmployeeInfo.Create();
                
                employeeInfo.WWID = employeeRubyInfo[Constants.Module.RubiWWIDIndex];
                employeeInfo.Login = employeeRubyInfo[Constants.Module.RubiLoginIndex];
                employeeInfo.BusinessEmail = employeeRubyInfo[Constants.Module.RubiBusinessEmailIndex];
                employeeInfo.Status = employeeRubyInfo[Constants.Module.RubiStatusIndex];
                employeeInfo.LastName = employeeRubyInfo[Constants.Module.RubiLastNameIndex];
                employeeInfo.FirstName = employeeRubyInfo[Constants.Module.RubiFirstNameIndex];
                employeeInfo.MiddleName = employeeRubyInfo[Constants.Module.RubiMiddleNameIndex];
                employeeInfo.PositionName = employeeRubyInfo[Constants.Module.RubiPositionNameIndex];
                employeeInfo.Divcode = employeeRubyInfo[Constants.Module.RubiDivcodeIndex];
                employeeInfo.ManagerWWID = employeeRubyInfo[Constants.Module.RubiManagerWWIDIndex];
                
                #region Преобразование даты.
                var startDate = DateTime.MinValue;
                
                if (!string.IsNullOrEmpty(employeeRubyInfo[Constants.Module.RubiStartDateIndex]) &&
                    Calendar.TryParseDate(employeeRubyInfo[Constants.Module.RubiStartDateIndex], out startDate))
                  employeeInfo.StartDate = startDate;
                else
                  Logger.ErrorFormat("GetEmployeesFromCsv. Номер строки: {0}. Полученное значение: {1}. Не удалось преобразовать дату для поля StartDate.", linesNumber, employeeRubyInfo[Constants.Module.RubiStartDateIndex]);

                var endDate = DateTime.MinValue;
                
                if (!string.IsNullOrEmpty(employeeRubyInfo[Constants.Module.RubiEndDateIndex]))
                {
                  if (Calendar.TryParseDate(employeeRubyInfo[Constants.Module.RubiEndDateIndex], out endDate))
                    employeeInfo.EndDate = endDate;
                  else
                    Logger.ErrorFormat("GetEmployeesFromCsv. Номер строки: {0}. Полученное значение: {1}. Не удалось преобразовать дату для поля EndDate.", linesNumber, employeeRubyInfo[Constants.Module.RubiEndDateIndex]);
                }

                #endregion
                
                employeeInfoList.Add(employeeInfo);
              }
              catch (Exception ex)
              {
                Logger.ErrorFormat("GetEmployeesFromCsv. Номер строки: {0}. Строка с данными: {1}. Ошибка при чтении строки: {2}. StackTrace: {3}.",
                                   linesNumber, line, ex.Message, ex.StackTrace);
              }
            }
            Logger.DebugFormat("GetEmployeesFromCsv. Окончание чтения файла. Всего прочитанных строк: {0}.", linesNumber);
          }
        }
        #endregion
        
        #region Сортировка по WWID руководителя, для создания/изменения в первую очередь сотрудников без руководителя.
        employeeInfoList = employeeInfoList
          .OrderBy(e => e.ManagerWWID)
          .ThenBy(e => e.WWID)
          .ToList();
        
        #endregion
        
        #region Создание/обновление сотрудников в DirectumRX.
        // Список необработанных записей.
        var notSuccessList = new List<Structures.Module.IEmployeeInfo>();
        
        foreach (var employeeInfo in employeeInfoList)
        {
          try
          {
            #region Создание/получение сотрудника, его персоны и логина.
            
            // Получение сотрудника.
            var employee = DirRX.IntegrationRubi.Employees.GetAll(ei => ei.WWIDDirRX == employeeInfo.WWID).FirstOrDefault();
            
            // Если сотрудник не найден, то необходимо его создать вместе с персоной.
            if (employee == null)
            {
              employee = DirRX.IntegrationRubi.Employees.Create();
              Logger.DebugFormat("GetEmployeesFromCsv. Id сотрудника: {0}. Создан сотрудник.", employee.Id);
              employee.Person = Sungero.Parties.People.Create();
              Logger.DebugFormat("GetEmployeesFromCsv. Id сотрудника: {0}. Id персоны: {1}. Создана персона.", employee.Id, employee.Person.Id);
            }
            else
              Logger.DebugFormat("GetEmployeesFromCsv. Id сотрудника: {0}. Найден сотрудник.", employee.Id);
            
            var login = Logins.Null;
            
            // Создание/получение логина.
            if (!string.IsNullOrEmpty(employeeInfo.Login) && employee.Login == null)
            {
              login = Sungero.CoreEntities.Logins.Create();
              Logger.DebugFormat("GetEmployeesFromCsv. Id сотрудника: {0}. Id учётной записи: {1}. Создана учётная запись.", employee.Id, login.Id);
            }
            else
              login = employee.Login;
            
            #endregion
            
            #region Поиск подразделения, должности и руководителя сотрудника.
            
            // Поиск подразделения сотрудника.
            var department = Sungero.Company.Departments.Null;
            
            if (!string.IsNullOrEmpty(employeeInfo.Divcode))
            {
              department = Sungero.Company.Departments.GetAll(d => d.Code.Equals(employeeInfo.Divcode)).FirstOrDefault();
              
              if (department == null)
                Logger.DebugFormat("GetEmployeesFromCsv. Id сотрудника: {0}. Не найдено подразделение сотрудника в БД.", employee.Id);
              else
                Logger.DebugFormat("GetEmployeesFromCsv. Id сотрудника: {0}. Id подразделения: {1}. Не найдено подразделение сотрудника в БД.", employee.Id, department.Id);
            }
            
            // Поиск должности сотрудника.
            var jobTitle = Sungero.Company.JobTitles.Null;
            
            if (!string.IsNullOrEmpty(employeeInfo.PositionName))
            {
              jobTitle = Sungero.Company.JobTitles.GetAll(jt => jt.Name == employeeInfo.PositionName).FirstOrDefault();
              
              if (jobTitle == null)
                Logger.DebugFormat("GetEmployeesFromCsv. Id сотрудника: {0}. Не найдена должность сотрудника в БД.", employee.Id);
              else
                Logger.DebugFormat("GetEmployeesFromCsv. Id сотрудника: {0}. Id должности: {1}. Найдена должность сотрудника в БД.", employee.Id, jobTitle.Id);
            }
            
            // Поиск руководителя сотрудника.
            var manager = DirRX.IntegrationRubi.Employees.Null;
            
            if (!string.IsNullOrEmpty(employeeInfo.ManagerWWID))
            {
              manager = DirRX.IntegrationRubi.Employees
                .GetAll(e => e.WWIDDirRX.Equals(employeeInfo.ManagerWWID))
                .FirstOrDefault();
              
              if (manager == null)
                Logger.DebugFormat("GetEmployeesFromCsv. Id сотрудника: {0}. Не найден руководитель сотрудника в БД.", employee.Id);
              else
                Logger.DebugFormat("GetEmployeesFromCsv. Id сотрудника: {0}. Id руководителя: {1}. Найден руководитель сотрудника в БД.", employee.Id, manager.Id);
            }
            #endregion
            
            #region Изменение персоны.
            var comment = string.Empty;
            
            if (employee.Person.FirstName != employeeInfo.FirstName)
            {
              // TODO Добавить запись свойства в comment, аналогично ниже
              employee.Person.FirstName = employeeInfo.FirstName;
              comment = string.Concat(comment, "Изменено имя. ");
            }
            
            if (employee.Person.LastName != employeeInfo.LastName)
            {
              employee.Person.LastName = employeeInfo.LastName;
              comment = string.Concat(comment, "Изменена фамилия. ");
            }
            
            if (employee.Person.MiddleName != employeeInfo.MiddleName)
            {
              employee.Person.MiddleName = employeeInfo.MiddleName;
              comment = string.Concat(comment, "Изменено отчество. ");
            }
            
            // Сохранение в БД.
            if (employee.Person.State.IsChanged)
            {
              employee.Person.Save();
              Logger.DebugFormat("GetEmployeesFromCsv. Id сотрудника: {0}. Id персоны: {1}. Обновление персоны сотрудника. {2}", employee.Id, employee.Person.Id, comment);
            }
            #endregion
            
            #region Изменение учётной записи.
            
            // 
            if (login != null && !string.IsNullOrEmpty(employeeInfo.Login)  && login.LoginName != employeeInfo.Login)
            {
              login.LoginName = employeeInfo.Login;
              login.Save();
              Logger.DebugFormat("GetEmployeesFromCsv. Id сотрудника: {0}. Id учётной записи: {1}. Обновление учётной записи. Изменено имя учётной записи", employee.Id, login.Id);
            }
            
            #endregion
            
            #region Изменение свойств сотрудника.
            comment = string.Empty;
            
            // Изменение почты сотрудника.
            if (employee.Email != employeeInfo.BusinessEmail)
            {
              employee.Email = employeeInfo.BusinessEmail;
              Logger.DebugFormat("GetEmployeesFromCsv. Id сотрудника: {0}. Обновление сотрудника. Изменена почта сотрудника.", employee.Id);
            }
            
            // Изменение даты приёма сотрудника.
            if (employee.StartDateDirRX != employeeInfo.StartDate)
            {
              employee.StartDateDirRX = employeeInfo.StartDate;
              Logger.DebugFormat("GetEmployeesFromCsv. Id сотрудника: {0}. Обновление сотрудника. Изменена дата приёма сотрудника.", employee.Id);
            }
            
            // Изменение даты увольнения сотрудника.
            if (employee.EndDateDirRX != employeeInfo.EndDate)
            {
              employee.EndDateDirRX = employeeInfo.EndDate;
              Logger.DebugFormat("GetEmployeesFromCsv. Id сотрудника: {0}. Обновление сотрудника. Изменена дата увольнения сотрудника.", employee.Id);
            }
            
            // Изменение учётной записи у сотрудника.
            if (employee.Login != null && !Logins.Equals(employee.Login, login))
            {
              Logger.DebugFormat("GetEmployeesFromCsv. Id сотрудника: {0}. Обновление сотрудника. Изменение учётной записи: Старое значение {1}. Новое значение {2}", employee.Id, employee.Login, login);
              employee.Login = login;
            }
            
            // Изменение подразделения у сотрудника.
            if (employee.Department != null && !Sungero.Company.Departments.Equals(employee.Department, department))
            {
              Logger.DebugFormat("GetEmployeesFromCsv. Id сотрудника: {0}. Обновление сотрудника. Изменение подразделения: Старое значение {1}. Новое значение {2}", employee.Id, employee.Department, department);
              employee.Department = department;
            }
            
            // Изменение должности у сотрудника.
            
            if (employee.JobTitle != null && !Sungero.Company.JobTitles.Equals(employee.JobTitle, jobTitle))
            {
              Logger.DebugFormat("GetEmployeesFromCsv. Id сотрудника: {0}. Обновление сотрудника. Изменение должности: Старое значение {1}. Новое значение {2}", employee.Id, employee.JobTitle, jobTitle);
              employee.JobTitle = jobTitle;
            }
            
            // Изменение состояния сотрудника.
            var status = employeeInfo.Status == Constants.Module.EmployeeTerminatedStatus ?
              DirRX.IntegrationRubi.Employee.Status.Closed :
              DirRX.IntegrationRubi.Employee.Status.Active;
            
            if (employee.Status != status)
            {
              Logger.DebugFormat("GetEmployeesFromCsv. Id сотрудника: {0}. Обновление сотрудника. Изменено состояние - {1}.", employee.Id, employee.Status);
              employee.Status = status;
            }
            
            // Изменение руководителя сотрудника.
            if (employee.MangerDirRX != null && !DirRX.IntegrationRubi.Employees.Equals(employee.MangerDirRX, manager))
            {
              employee.MangerDirRX = manager;
              
              if (employee.MangerDirRX == null)
                Logger.DebugFormat("GetEmployeesFromCsv. Id сотрудника: {0}. Обновление сотрудника. Очищен руководитель сотрудника.", employee.Id);
              else
                Logger.DebugFormat("GetEmployeesFromCsv. Id сотрудника: {0}. Id руководителя: {1}. Обновление сотрудника. Изменен руководитель сотрудника.", employee.Id, employee.MangerDirRX.Id);
            }
            #endregion
            
            #region Сохранение сотрудника в БД.
            
            if (employee.State.IsChanged)
            {
              // Выключение обязательности заполнения подразделения и должности.
              employee.State.Properties.Department.IsRequired = false;
              employee.State.Properties.JobTitle.IsRequired = false;
              
              try
              {
                employee.Save();
                Logger.DebugFormat("GetEmployeesFromCsv. Id сотрудника: {0}. Сотрудник сохранён в БД.", employee.Id);
              }
              catch (Exception ex)
              {
                Logger.ErrorFormat("GetEmployeesFromCsv. Id сотрудника: {0}. Ошибка при сохранении сотрудника в БД: {1}, {2}", employee.Id, ex.Message, ex.StackTrace);
              }
              // Включение обязательности заполнения подразделения и должности.
              employee.State.Properties.Department.IsRequired = true;
              employee.State.Properties.JobTitle.IsRequired = true;
            }
            #endregion
            
          }
          catch(Exception ex)
          {
            Logger.ErrorFormat("GetEmployeesFromCsv. Ошибка при создании/обновлении сотрудника WWID = {0}: {1}, {2}",
                               employeeInfo.WWID, ex.Message, ex.StackTrace);
            
            // Добавление необработанной записи в список notSuccessList.
            notSuccessList.Add(employeeInfo);
          }
        }
        #endregion
        
        #region Логирование результатов интеграции.
        Logger.DebugFormat("GetEmployeesFromCsv. Обработано {0} записей. Успешно обновлено {1}. Не удалось обновить: {2}.",
                           linesNumber, linesNumber - notSuccessList.Count, notSuccessList.Count);
        #endregion
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("GetEmployeesFromCsv. Ошибка при выполнении фонового процесса: {0}, {1}", ex.Message, ex.StackTrace);
      }
      Logger.Debug("GetEmployeesFromCsv. Конец процесса.");
    }
  }
}
