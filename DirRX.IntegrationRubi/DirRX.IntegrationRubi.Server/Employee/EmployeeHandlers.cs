using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.IntegrationRubi.Employee;

namespace DirRX.IntegrationRubi
{
  partial class EmployeeServerHandlers
  {

    public override void BeforeSaveHistory(Sungero.Domain.HistoryEventArgs e)
    {
      var IsEmployeeCreated = e.Action == Sungero.CoreEntities.History.Action.Create;
      var IsEmployeeUpdated = e.Action == Sungero.CoreEntities.History.Action.Update;

      if (IsEmployeeCreated || IsEmployeeUpdated)
      {
        var commentsList = new List<string>();
        
        // Изменилось WWID сотрудника.
        if (_obj.State.Properties.WWIDDirRX.IsChanged)
        {
          commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
            Employees.Info.Properties.WWIDDirRX.LocalizedName, _obj.WWIDDirRX));
        }
        
        // Изменился логин сотрудника.
        if (_obj.State.Properties.Login.IsChanged)
        {
          commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.EntityIsChangedCommentFormat(
            Employees.Info.Properties.Login.LocalizedName,
            _obj.Login.LoginName,
            _obj.Login.Id));
        }
        
        // Изменилась почта сотрудника.
        if (_obj.State.Properties.Email.IsChanged)
        {
          if (_obj.Email == null)
          {
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsClearedFormat(
              Employees.Info.Properties.Email.LocalizedName));
          }
          else
          {
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
              Employees.Info.Properties.Email.LocalizedName, _obj.Email));
          }
        }
        
        // Изменилось состояние сотрудника.
        if (_obj.State.Properties.Status.IsChanged)
        {
          commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
            Employees.Info.Properties.Status.LocalizedName,
            Employees.Info.Properties.Status.GetLocalizedValue(_obj.Status)));
        }
        
        // Изменилась дата приёма сотрудника.
        if (_obj.State.Properties.StartDateDirRX.IsChanged)
        {
          commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
            Employees.Info.Properties.StartDateDirRX.LocalizedName, _obj.StartDateDirRX));
        }
        
        // Изменилась дата увольнения сотрудника.
        if (_obj.State.Properties.EndDateDirRX.IsChanged)
        {
          if (_obj.EndDateDirRX == null)
          {
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsClearedFormat(
              Employees.Info.Properties.EndDateDirRX.LocalizedName));
          }
          else
          {
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
              Employees.Info.Properties.EndDateDirRX.LocalizedName, _obj.EndDateDirRX));
          }
        }
        
        // Изменилось ФИО.
        if (_obj.State.Properties.Person.IsChanged)
        {
          commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PersonIsChangedFormat(
            _obj.Person.LastName, _obj.Person.FirstName, _obj.Person.MiddleName, _obj.Person.Id));
        }
        
        // Изменилось подразделение сотрудника.
        if (_obj.State.Properties.Department.IsChanged)
        {
          if (_obj.Department == null)
          {
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsClearedFormat(
              Employees.Info.Properties.Department.LocalizedName));
          }
          else
          {
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.EntityIsChangedCommentFormat(
              Employees.Info.Properties.Department.LocalizedName,
              _obj.Department.Name,
              _obj.Department.Id));
          }
        }
        
        // Изменилась должность сотрудника.
        if (_obj.State.Properties.JobTitle.IsChanged)
        {
          if (_obj.JobTitle == null)
          {
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsClearedFormat(
              Employees.Info.Properties.JobTitle.LocalizedName));
          }
          else
          {
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.EntityIsChangedCommentFormat(
              Employees.Info.Properties.JobTitle.LocalizedName,
              _obj.JobTitle.Name,
              _obj.JobTitle.Id));
          }
        }
        
        // Изменился руководитель сотрудника.
        if (_obj.State.Properties.MangerDirRX.IsChanged)
        {
          if (_obj.MangerDirRX == null)
          {
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsClearedFormat(
              Employees.Info.Properties.MangerDirRX.LocalizedName));
          }
          else
          {
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.ManagerIsChangedFormat(
              _obj.MangerDirRX.Person.LastName,
              _obj.MangerDirRX.Person.FirstName,
              _obj.MangerDirRX.Person.MiddleName,
              _obj.MangerDirRX.Person.Id));
          }
        }
        
        var comment = string.Join(" ", commentsList);
        
        if (IsEmployeeCreated)
        {
          var createOperation = new Enumeration(Constants.Company.Employee.Created);
          var createOperationDetails = createOperation;
          e.Write(createOperation, createOperationDetails, comment);
        }
        
        if (IsEmployeeUpdated)
        {
          var updateOperation = new Enumeration(Constants.Company.Employee.Updated);
          var updateOperationDetails = updateOperation;
          e.Write(updateOperation, updateOperationDetails, comment);
        }
      }
    }
  }
}