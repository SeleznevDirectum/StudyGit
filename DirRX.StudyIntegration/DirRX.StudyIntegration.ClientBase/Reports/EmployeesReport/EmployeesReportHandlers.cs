using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.StudyIntegration
{
  partial class EmployeesReportClientHandlers
  {

    public override void BeforeExecute(Sungero.Reporting.Client.BeforeExecuteEventArgs e)
    {
       // Создать диалог с запросом периода дат.
      var dialog = Dialogs.CreateInputDialog("Параметры отчета");
      var startDate = dialog.AddDate("Дата от", true , Calendar.Today.AddYears(-1));
      var endDate = dialog.AddDate("Дата по", false, Calendar.Today);
                
      if (dialog.Show() != DialogButtons.Ok)
        e.Cancel = true;
   
      EmployeesReport.StartDate = startDate.Value.Value;
      
      if (endDate.Value != null)
        EmployeesReport.EndDate = endDate.Value.Value;
    }

  }
}