// <copyright file="ExceptionExtensions.cs" company="CGI">
//     Copyright (c) 2016 CGI. All rights reserved.
// </copyright>
// <authors>
//     <author>Nathan, Nicolas</author>
// </authors>
// <summary>Copyright header</summary>
// <date>19/10/2016 11:45</date>
using System;
using System.Text;

namespace WpfCrowdDetection.Helper
{
    /// <summary>
    /// Classe d'extension pour le type <see cref="Exception"/>
    /// </summary>
    public static class ExceptionExtensions
    {
        #region Public Methods

        public static string FlattenException(this Exception exception)
        {
            var stringBuilder = new StringBuilder();

            while (exception != null)
            {
                stringBuilder.AppendLine(exception.Message);
                stringBuilder.AppendLine(exception.StackTrace);

                exception = exception.InnerException;
            }

            return stringBuilder.ToString();
        }

        #endregion Public Methods
    }
}