// <copyright file="GlobalSuppressions.cs" company="Meld contributors">
//  Copyright (c) Meld contributors. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Meld", Justification = "No.")]
[assembly: SuppressMessage(
    "Microsoft.Design",
    "CA1020:AvoidNamespacesWithFewTypes",
    Scope = "namespace",
    Target = "System.Data.SqlClient",
    Justification = "I will not.")]
