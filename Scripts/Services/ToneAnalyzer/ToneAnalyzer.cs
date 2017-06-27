﻿/**
* Copyright 2015 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/

using System.Collections.Generic;
using IBM.Watson.DeveloperCloud.Connection;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.Logging;
using System.Text;
using MiniJSON;
using System;
using FullSerializer;

namespace IBM.Watson.DeveloperCloud.Services.ToneAnalyzer.v3
{
    /// <summary>
    /// This class wraps the Tone Analyzer service.
    /// <a href="http://www.ibm.com/watson/developercloud/tone-analyzer.html">Tone Analyzer Service</a>
    /// </summary>
    public class ToneAnalyzer : IWatsonService
    {
        #region Private Data
        private const string SERVICE_ID = "ToneAnalyzerV3";
        private static fsSerializer _serializer = new fsSerializer();
        private Credentials _credentials = null;
        private string _url = "https://gateway.watsonplatform.net/tone-analyzer/api";
        private string _versionDate;
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets and sets the endpoint URL for the service.
        /// </summary>
        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        /// <summary>
        /// Gets and sets the versionDate of the service.
        /// </summary>
        public string VersionDate
        {
            get { return _versionDate; }
            set { _versionDate = value; }
        }

        /// <summary>
        /// Gets and sets the credentials of the service.
        /// </summary>
        public Credentials Credentials
        {
            get { return _credentials; }
            set { _credentials = value; }
        }
        #endregion

        #region Constructor
        public ToneAnalyzer(Credentials credentials)
        {
            Credentials = credentials;
        }
        #endregion

        #region Get Tone
        private const string FUNCTION_TONE = "/v3/tone";

        /// <summary>
        /// The Get Tone Analyzed callback delegate.
        /// </summary>
        /// <param name="resp"></param>
        /// <param name="data"></param>
        public delegate void OnGetToneAnalyzed(ToneAnalyzerResponse resp, string data);

        /// <summary>
        /// Gets the tone analyze.
        /// </summary>
        /// <returns><c>true</c>, if tone analyze was gotten, <c>false</c> otherwise.</returns>
        /// <param name="callback">Callback.</param>
        /// <param name="text">Text.</param>
        /// <param name="data">Data.</param>
        public bool GetToneAnalyze(OnGetToneAnalyzed callback, string text, string data = null)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            RESTConnector connector = RESTConnector.GetConnector(Credentials, _url, FUNCTION_TONE);
            if (connector == null)
                return false;

            GetToneAnalyzerRequest req = new GetToneAnalyzerRequest();
            req.Callback = callback;
            req.OnResponse = GetToneAnalyzerResponse;

            Dictionary<string, string> upload = new Dictionary<string, string>();
            upload["text"] = "\"" + text + "\"";
            req.Send = Encoding.UTF8.GetBytes(Json.Serialize(upload));
            req.Data = data;
            req.Headers["Content-Type"] = "application/json";
            req.Parameters["version"] = "2016-02-11";
            req.Parameters["sentences"] = "true";
            return connector.Send(req);
        }

        private class GetToneAnalyzerRequest : RESTConnector.Request
        {
            public string Data { get; set; }
            public OnGetToneAnalyzed Callback { get; set; }
        };

        private void GetToneAnalyzerResponse(RESTConnector.Request req, RESTConnector.Response resp)
        {
            ToneAnalyzerResponse response = new ToneAnalyzerResponse();
            if (resp.Success)
            {
                try
                {
                    fsData data = null;
                    fsResult r = fsJsonParser.Parse(Encoding.UTF8.GetString(resp.Data), out data);
                    if (!r.Succeeded)
                        throw new WatsonException(r.FormattedMessages);

                    object obj = response;
                    r = _serializer.TryDeserialize(data, obj.GetType(), ref obj);
                    if (!r.Succeeded)
                        throw new WatsonException(r.FormattedMessages);
                }
                catch (Exception e)
                {
                    Log.Error("ToneAnalyzer", "GetToneAnalyzerResponse Exception: {0}", e.ToString());
                    resp.Success = false;
                }
            }

            if (((GetToneAnalyzerRequest)req).Callback != null)
                ((GetToneAnalyzerRequest)req).Callback(resp.Success ? response : null, ((GetToneAnalyzerRequest)req).Data);
        }


        #endregion

        #region IWatsonService interface
        /// <exclude />
        public string GetServiceID()
        {
            return SERVICE_ID;
        }

        /// <exclude />
        public void GetServiceStatus(ServiceStatus callback)
        {
            if (Utilities.Config.Instance.FindCredentials(SERVICE_ID) != null)
                new CheckServiceStatus(this, callback);
            else
                callback(SERVICE_ID, false);
        }

        private class CheckServiceStatus
        {
            private ToneAnalyzer service = null;
            private ServiceStatus callback = null;

            public CheckServiceStatus(ToneAnalyzer service, ServiceStatus callback)
            {
                this.service = service;
                this.callback = callback;

                if (!this.service.GetToneAnalyze(this.OnGetToneAnalyzed, "Test"))
                    this.callback(SERVICE_ID, false);
            }

            private void OnGetToneAnalyzed(ToneAnalyzerResponse resp, string data)
            {
                if (callback != null)
                    callback(SERVICE_ID, resp != null);
            }
        };
        #endregion
    }
}

