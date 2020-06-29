// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace MobileTest.Framework
{
    public static class TestConfigHelper
    {
        public static void SetAutoConfig(this TestHarness harness, TestConfig config)
        {
            harness.Settings.ManualMode = true;

            if (config == null)
            {
                harness.Settings.ManualMode = false;
                return;
            }

            harness.Settings.Custom["MobileServiceRuntimeUrl"] = config.MobileServiceRuntimeUrl;
            harness.Settings.Custom["TestFrameworkStorageContainerSasToken"] = config.TestFrameworkStorageContainerSasToken;
            harness.Settings.Custom["TestFrameworkStorageContainerUrl"] = config.TestFrameworkStorageContainerUrl;
            harness.Settings.Custom["RuntimeVersion"] = config.RuntimeVersion;
            harness.Settings.TagExpression = config.TagExpression;
            harness.Settings.ManualMode = false;
        }


        /// <summary>
        /// Set the test harness configuration for manual or auto mode. Sets the app to  auto mode
        /// if more than 2 or more arguments are passed.
        /// </summary>
        /// <param name="harness">Test Harness object to be configured</param>
        /// <param name="Arguments">String representing the arguments to be configuered seperated by space.Like tags:todo</param>
        public static void SetAutoConfig(this TestHarness harness, String Arguments)
        {
            harness.Settings.ManualMode = true;

            if (String.IsNullOrEmpty(Arguments))
            {
                return;
            }

            var argsArray = Arguments.Split(' ');

            if (argsArray.Count() < 2)
            {
                return;
            }
            var args = parseArguments(argsArray);

            if (args.Count > 0)
            {
                harness.Settings.Custom["MobileServiceRuntimeUrl"] = getArgumentValue("url", args);
                harness.Settings.Custom["TestFrameworkStorageContainerUrl"] = getArgumentValue("storageurl", args);
                harness.Settings.Custom["TestFrameworkStorageContainerSasToken"] = getArgumentValue("storagesastoken", args);
                harness.Settings.Custom["RuntimeVersion"] = getArgumentValue("runtimeversion", args);
                harness.Settings.TagExpression = getArgumentValue("tags", args);
                harness.Settings.ManualMode = false;
            }
        }
        /// <summary>
         /// Set the test harness configuration for manual or auto mode. Sets the app to  auto mode
         /// if more than 2 or more arguments are passed.
         /// </summary>
         /// <param name="harness">Test Harness object to be configured</param>
         /// <param name="queryString">Query string containing the parameters to be used</param>
        public static void SetAutoConfigQueryString(this TestHarness harness, String queryString)
        {
            harness.Settings.ManualMode = true;

            if (String.IsNullOrEmpty(queryString))
            {
                return;
            }

            var args = parseQueryString(queryString);

            if (args.Count > 0)
            {
                harness.Settings.Custom["MobileServiceRuntimeUrl"] = getArgumentValue("url", args);
                harness.Settings.Custom["TestFrameworkStorageContainerUrl"] = getArgumentValue("storageurl", args);
                harness.Settings.Custom["TestFrameworkStorageContainerSasToken"] = getArgumentValue("storagesastoken", args);
                harness.Settings.Custom["RuntimeVersion"] = getArgumentValue("runtimeversion", args);
                harness.Settings.TagExpression = getArgumentValue("tags", args);
                harness.Settings.ManualMode = false;
            }
        }

        private static string getArgumentValue(string name, Dictionary<string, string> args)
        {
            string argValue = string.Empty;
            args.TryGetValue(name, out argValue);
            return argValue;
        }

        /// <summary>
        /// Parse the arguments received
        /// </summary>
        /// <param name="args">String containing the arguments</param>
        /// <returns>Dictionary representing the arguments collection</returns>
        private static Dictionary<string, string> parseArguments(string[] args)
        {
            var parameters = new Dictionary<string, string>();
            foreach (var arg in args)
            {
                int index = arg.IndexOf(':');

                if (index < 0 || index + 1 == arg.Length || index == 0)
                    continue;

                string argName = arg.Substring(0, index).ToLower();
                string argVal = arg.Substring(index + 1);
                parameters.Add(argName, argVal);
            }
            return parameters;
        }

        /// <summary>
        /// Parse the query string received
        /// </summary>
        /// <param name="query">String containing the query string</param>
        /// <returns>Dictionary representing the arguments collection</returns>
        private static Dictionary<string, string> parseQueryString(string query)
        {
            if (String.IsNullOrEmpty(query))
            {
                throw new ArgumentException("Query string must not be empty or null");
            }

            if (query.IndexOf("?") == 0)
            {
                query = query.Substring(1);
            }

            var args = query.Split('&');

            var parameters = new Dictionary<string, string>();
            foreach (var arg in args)
            {
                int index = arg.IndexOf('=');

                if (index < 0 || index + 1 == arg.Length || index == 0)
                    continue;

                string argName = arg.Substring(0, index).ToLower();
                string argVal = Uri.UnescapeDataString(arg.Substring(index + 1));
                parameters.Add(argName, argVal);
            }
            return parameters;
        }
    }
}
