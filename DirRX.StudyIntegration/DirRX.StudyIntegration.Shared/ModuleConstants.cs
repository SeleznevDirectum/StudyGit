using System;
using Sungero.Core;

namespace DirRX.StudyIntegration.Constants
{
  public static class Module
  {

    /// <summary>
    /// Формат даты в csv файле.
    /// </summary>
    public const string DateTimeParseFormat = "dd.MM.yyyy";

    /// <summary>
    /// Символ разделителя в csv файле.
    /// </summary>
    public const char RubiCsvSplitSymbol = ';';

    /// <summary>
    /// Индекс атрибута ManagerWWID в csv файле.
    /// </summary>
    public const int RubiManagerWWIDIndex = 11;

    /// <summary>
    /// Индекс атрибута Divcode в csv файле.
    /// </summary>
    public const int RubiDivcodeIndex = 10;

    /// <summary>
    /// Индекс атрибута PositionName в csv файле.
    /// </summary>
    public const int RubiPositionNameIndex = 9;

    /// <summary>
    /// Индекс атрибута MiddleName в csv файле.
    /// </summary>
    public const int RubiMiddleNameIndex = 8;

    /// <summary>
    /// Индекс атрибута FirstName в csv файле.
    /// </summary>
    public const int RubiFirstNameIndex = 7;

    /// <summary>
    /// Индекс атрибута LastName в csv файле.
    /// </summary>
    public const int RubiLastNameIndex = 6;

    /// <summary>
    /// Индекс атрибута EndDate в csv файле.
    /// </summary>
    public const int RubiEndDateIndex = 5;

    /// <summary>
    /// Индекс атрибута StartDate в csv файле.
    /// </summary>
    public const int RubiStartDateIndex = 4;

    /// <summary>
    /// Индекс атрибута Status в csv файле.
    /// </summary>
    public const int RubiStatusIndex = 3;

    /// <summary>
    /// Индекс атрибута BusinessEmail в csv файле.
    /// </summary>
    public const int RubiBusinessEmailIndex = 2;

    /// <summary>
    /// Индекс атрибута Login в csv файле.
    /// </summary>
    public const int RubiLoginIndex = 1;

    /// <summary>
    /// Индекс атрибута WWID в csv файле.
    /// </summary>
    public const int RubiWWIDIndex = 0;

    /// <summary>
    /// Статус закрытой карточки сотрудника в системе Rubi.
    /// </summary>
    public const string EmployeeTerminatedStatus = "terminated";

  }
}