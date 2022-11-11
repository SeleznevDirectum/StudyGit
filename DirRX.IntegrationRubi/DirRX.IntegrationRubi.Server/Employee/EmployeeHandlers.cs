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
        if (_obj.State.Properties.WWIDDirRX.IsChanged &&
            _obj.State.Properties.WWIDDirRX.PreviousValue != _obj.WWIDDirRX)
          commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
            Employees.Info.Properties.WWIDDirRX.LocalizedName, _obj.WWIDDirRX));
        
        
        // Изменился логин сотрудника.
        if (_obj.State.Properties.Login.IsChanged && !Logins.Equals(_obj.State.Properties.Login.OriginalValue, _obj.Login))
        {
          if (_obj.Login == null)
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsClearedFormat(
              Employees.Info.Properties.Login.LocalizedName));
          else
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.EntityIsChangedCommentFormat(
              Employees.Info.Properties.Login.LocalizedName,
              _obj.Login.LoginName,
              _obj.Login.Id));
        }
        
        // Изменилась почта сотрудника.
        if (_obj.State.Properties.Email.IsChanged && _obj.State.Properties.Email.PreviousValue != _obj.Email)
        {
          if (_obj.Email == null)
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsClearedFormat(
              Employees.Info.Properties.Email.LocalizedName));
          else
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
              Employees.Info.Properties.Email.LocalizedName, _obj.Email));
        }
        
        // Изменилось состояние сотрудника.
        if (_obj.State.Properties.Status.IsChanged && _obj.State.Properties.Status.PreviousValue != _obj.Status)
        {
          commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
            Employees.Info.Properties.Status.LocalizedName,
            Employees.Info.Properties.Status.GetLocalizedValue(_obj.Status)));
        }
        
        // Изменилась дата приёма сотрудника.
        if (_obj.State.Properties.StartDateDirRX.IsChanged &&
            _obj.State.Properties.StartDateDirRX.PreviousValue != _obj.StartDateDirRX)
          commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
            Employees.Info.Properties.StartDateDirRX.LocalizedName, _obj.StartDateDirRX));
        
        // Изменилась дата увольнения сотрудника.
        if (_obj.State.Properties.EndDateDirRX.IsChanged &&
            _obj.State.Properties.EndDateDirRX.PreviousValue != _obj.EndDateDirRX)
        {
          if (_obj.EndDateDirRX == null)
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsClearedFormat(
              Employees.Info.Properties.EndDateDirRX.LocalizedName));
          else
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
              Employees.Info.Properties.EndDateDirRX.LocalizedName, _obj.EndDateDirRX));
        }
        
        // Изменился табельный номер сотрудника.
        if (_obj.State.Properties.PersonnelNumber.IsChanged &&
            _obj.State.Properties.PersonnelNumber.PreviousValue != _obj.PersonnelNumber)
        {
          if (_obj.PersonnelNumber == null)
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsClearedFormat(
              Employees.Info.Properties.PersonnelNumber.LocalizedName));
          else
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
              Employees.Info.Properties.PersonnelNumber.LocalizedName, _obj.PersonnelNumber));
        }
        
        // Изменился рабочий телефон сотрудника.
        if (_obj.State.Properties.Phone.IsChanged && _obj.State.Properties.Phone.PreviousValue != _obj.Phone)
        {
          if(_obj.Phone == null)
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsClearedFormat(
              Employees.Info.Properties.Phone.LocalizedName));
          else
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
              Employees.Info.Properties.Phone.LocalizedName, _obj.Phone));
        }
        
        // Изменилось примечание у сотрудника.
        if (_obj.State.Properties.Note.IsChanged && _obj.State.Properties.Note.PreviousValue != _obj.Note)
        {
          if(_obj.Note == null)
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsClearedFormat(
              Employees.Info.Properties.Note.LocalizedName));
          else
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
              Employees.Info.Properties.Note.LocalizedName, _obj.Note));
        }
        
        // Изменилось описание у сотрудника.
        if (_obj.State.Properties.Description.IsChanged && _obj.State.Properties.Description.PreviousValue != _obj.Description)
        {
          if(_obj.Description == null)
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsClearedFormat(
              Employees.Info.Properties.Description.LocalizedName));
          else
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
              Employees.Info.Properties.Description.LocalizedName, _obj.Description));
        }
        
        // Изменилась фотография сотрудника.
        if (_obj.State.Properties.PersonalPhoto.IsChanged)
        {
          if (_obj.PersonalPhoto == null)
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PersonalPhotoIsCleared);
          else
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PersonalPhotoIsChanged);
        }
        
        // Изменился чебокс уведомлять о просроченных заданиях.
        if (_obj.State.Properties.NeedNotifyExpiredAssignments.IsChanged
            && _obj.State.Properties.NeedNotifyExpiredAssignments.PreviousValue != _obj.NeedNotifyExpiredAssignments)
          commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
            Employees.Info.Properties.NeedNotifyExpiredAssignments.LocalizedName, _obj.NeedNotifyExpiredAssignments));
        
        // Изменился чебокс уведомлять о новых заданиях.
        if (_obj.State.Properties.NeedNotifyNewAssignments.IsChanged
            && _obj.State.Properties.NeedNotifyNewAssignments.PreviousValue != _obj.NeedNotifyNewAssignments)
          commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
            Employees.Info.Properties.NeedNotifyNewAssignments.LocalizedName, _obj.NeedNotifyNewAssignments));
        
        // Изменился чебокс уведомлять о текущих заданиях и задачах.
        if (_obj.State.Properties.NeedNotifyAssignmentsSummary.IsChanged
            && _obj.State.Properties.NeedNotifyAssignmentsSummary.PreviousValue != _obj.NeedNotifyAssignmentsSummary)
          commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
            Employees.Info.Properties.NeedNotifyAssignmentsSummary.LocalizedName, _obj.NeedNotifyAssignmentsSummary));
        
        // Изменилось ФИО.
        if ((_obj.State.Properties.Person.IsChanged &&
             !Sungero.Parties.People.Equals(_obj.State.Properties.Person.PreviousValue, _obj.Person)) ||
            _obj.State.Properties.Name.OriginalValue != _obj.Name)
          commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PersonIsChangedFormat(
            _obj.Person, _obj.Id));
        
        // Изменилось подразделение сотрудника.
        if (_obj.State.Properties.Department.IsChanged &&
            !Sungero.Company.Departments.Equals(_obj.State.Properties.Department.PreviousValue, _obj.Department))
        {
          if (_obj.Department == null)
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsClearedFormat(
              Employees.Info.Properties.Department.LocalizedName));
          else
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.EntityIsChangedCommentFormat(
              Employees.Info.Properties.Department.LocalizedName,
              _obj.Department,
              _obj.Department.Id));
        }
        
        // Изменилась должность сотрудника.
        if (_obj.State.Properties.JobTitle.IsChanged &&
            !Sungero.Company.JobTitles.Equals(_obj.State.Properties.JobTitle.PreviousValue, _obj.JobTitle))
        {
          if (_obj.JobTitle == null)
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsClearedFormat(
              Employees.Info.Properties.JobTitle.LocalizedName));
          else
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.EntityIsChangedCommentFormat(
              Employees.Info.Properties.JobTitle.LocalizedName,
              _obj.JobTitle,
              _obj.JobTitle.Id));
        }
        
        // Изменился руководитель сотрудника.
        if (_obj.State.Properties.MangerDirRX.IsChanged
            && !Employees.Equals(_obj.State.Properties.MangerDirRX.PreviousValue, _obj.MangerDirRX))
        {
          if (_obj.MangerDirRX == null)
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.PropertyIsClearedFormat(
              Employees.Info.Properties.MangerDirRX.LocalizedName));
          else
            commentsList.Add(DirRX.IntegrationRubi.Employees.Resources.ManagerIsChangedFormat(
              _obj.MangerDirRX,
              _obj.MangerDirRX.Person.Id));
        }
        
        if (commentsList.Any())
        {
          var comment = string.Join(Environment.NewLine, commentsList);
          
          var operation = IsEmployeeCreated?
            new Enumeration(Constants.Company.Employee.Created) :
            new Enumeration(Constants.Company.Employee.Updated);
          
          var operationDetails = operation;
          e.Write(operation, operationDetails, comment);
        }
      }
    }
  }
}