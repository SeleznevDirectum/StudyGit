using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.StudyIntegration.Structures.Module
{

  /// <summary>
  /// 
  /// </summary>
  [Public]
  partial class EmployeeInfo
  {
    public string WWID {get; set;}
    public string Login {get; set;}
    public string BusinessEmail {get; set;}
    public string Status {get; set;}
    public string StartDate {get; set;}
    public string EndDate {get; set;}
    public string LastName {get; set;}
    public string FirstName {get; set;}
    public string MiddleName {get; set;}
    public string PositionName {get; set;} 
    public string Divcode {get; set;}
    public string ManagerWWID {get; set;}
   }

}