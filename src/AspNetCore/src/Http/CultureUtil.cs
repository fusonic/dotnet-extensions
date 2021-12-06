// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Fusonic.Extensions.AspNetCore.Http;

public static class CultureUtil
{
    /// <summary>
    /// See <see cref="FromAcceptLanguageHeader(IList{StringWithQualityHeaderValue},IEnumerable{CultureInfo},CultureInfo)"/>
    /// </summary>
    /// <param name="httpContext">Current http context</param>
    /// <param name="supportedCultures">Supported cultures</param>
    /// <param name="defaultCulture">Default gets returned if no culture is supported.</param>
    public static CultureInfo? FromAcceptLanguageHeader(HttpContext httpContext, IEnumerable<CultureInfo> supportedCultures, CultureInfo defaultCulture)
        => FromAcceptLanguageHeader(httpContext.Request.GetTypedHeaders().AcceptLanguage, supportedCultures, defaultCulture);

    /// <summary>
    /// Returns the first supported culture from the Accept-Language header. Weights from the header are taken into account. This also uses the "fallback to child"-method
    /// from GetFirstSupportedCulture().
    /// </summary>
    /// <example>
    /// //Header contains (sorted) de-DE,en-GB. Supported Cultures=de-AT,en-GB -> Returns de-AT
    /// //Header contains fr-FR. Supported Cultures=de-AT,en-GB, Default=de-AT -> Returns de-AT
    /// //Header contains en-GB. Supported Cultures=de-AT,en-GB, Default=de-AT -> Returns en-GB
    /// CultureUtil.FromAcceptLanguageHeader(HttpContext.Request.GetTypedHeaders().AcceptLanguage, appSettings.SupportedCultures, appSettings.DefaultCulture)</example>
    /// <param name="header">Accept-Language header. (HttpContext.Request.GetTypedHeaders().AcceptLanguage)</param>
    /// <param name="supportedCultures">Supported cultures</param>
    /// <param name="defaultCulture">Default gets returned if no culture is supported.</param>
    public static CultureInfo? FromAcceptLanguageHeader(IList<StringWithQualityHeaderValue> header, IEnumerable<CultureInfo> supportedCultures, CultureInfo defaultCulture)
    {
        if (header == null || header.Count == 0)
            return defaultCulture;

        var cultures = supportedCultures.ToList();
        var orderedLanguages = header.OrderByDescending(h => h, StringWithQualityHeaderValueComparer.QualityComparer)
                                     .Select(x => x.Value)
                                     .ToList();

        foreach (var language in orderedLanguages)
        {
            var cultureInfo = GetFirstSupportedCulture(language.Value, cultures, defaultCulture: null);

            if (cultureInfo != null && !Equals(cultureInfo, CultureInfo.InvariantCulture))
                return cultureInfo;
        }

        return defaultCulture;
    }

    /// <summary>
    /// Returns the first supported culture with a "fallback-to-child" method.
    /// </summary>
    /// <param name="culture">Culture to search</param>
    /// <param name="supportedCultures">Supported cultures</param>
    /// <param name="defaultCulture">Default gets returned if no culture is supported.</param>
    public static CultureInfo? GetFirstSupportedCulture(string culture, IEnumerable<CultureInfo> supportedCultures, CultureInfo? defaultCulture = null)
    {
        try
        {
            return GetFirstSupportedCulture(new CultureInfo(culture), supportedCultures.ToList(), defaultCulture);
        }
        catch (CultureNotFoundException)
        {
            //ignore invalid cultures. They are obviously not in the supported list.
            return defaultCulture;
        }
    }

    private static CultureInfo? GetFirstSupportedCulture(CultureInfo culture, List<CultureInfo> supportedCultures, CultureInfo? defaultCulture)
    {
        if (culture.Equals(CultureInfo.InvariantCulture))
            return defaultCulture;

        if (supportedCultures.Contains(culture))
            return culture;

        //this is more a "fall back to child". We prefer a specific culture to a neutral one. If we already are at a neutral one, get the first specific one
        //for example: Supported is de-AT, user comes with de-DE, falls back to 'de' (<- we are here), we want to return de-AT (so convert de-DE to de-AT)
        if (culture.IsNeutralCulture)
        {
            var firstSpecific = supportedCultures.FirstOrDefault(c => !c.IsNeutralCulture && c.Parent.Equals(culture));
            return firstSpecific ?? defaultCulture;
        }
        else
        {
            return GetFirstSupportedCulture(culture.Parent, supportedCultures, defaultCulture);
        }
    }
}
