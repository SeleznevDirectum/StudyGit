using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.StudyIntegration.Structures.Module
{

  /// <summary>
  /// Структура, описывающая csv файл из RUBI. 
  /// </summary>
  [Public]
  partial class EmployeeInfo
  {
    public string WWID {get; set;}
    public string Login {get; set;}
    public string BusinessEmail {get; set;}
    public string Status {get; set;}
    public DateTime? StartDate {get; set;}
    public DateTime? EndDate {get; set;}
    public string LastName {get; set;}
    public string FirstName {get; set;}
    public string MiddleName {get; set;}
    public string PositionName {get; set;} 
    public string Divcode {get; set;}
    public string ManagerWWID {get; set;}
   }

}