// ***********************************************************************
// Assembly         : ControlReplacer
// Author           : Jonathan Sheely
// Created          : 02-17-2014
//
// Last Modified By : Jonathan Sheely
// Last Modified On : 02-17-2014
// ***********************************************************************
// <copyright file="ControlReplacer.cs" company="InspectorIT">
//     Copyright (c) 2014. All rights reserved.
// </copyright>
// <summary>Class library to take a string input with tokens and replace with LiteralControls and UserControls</summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace InspectorIT
{
    /// <summary>
    /// Class Token.
    /// </summary>
    internal class Token
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Token"/> class.
        /// </summary>
        public Token()
        {
            UniqueId = Guid.NewGuid().ToString().Replace("-", "");
            Attributes = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>The unique identifier.</value>
        private string UniqueId { get; set; }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id
        {
            get { return TokenName + "_" + UniqueId; }
        }

        /// <summary>
        /// Gets or sets the start.
        /// </summary>
        /// <value>The start.</value>
        public int Start { get; set; }
        /// <summary>
        /// Gets or sets the end.
        /// </summary>
        /// <value>The end.</value>
        public int End { get; set; }
        /// <summary>
        /// Gets or sets the name of the token.
        /// </summary>
        /// <value>The name of the token.</value>
        public string TokenName { get; set; }
        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        /// <value>The attributes.</value>
        public Dictionary<string, string> Attributes { get; set; }
    }

    /// <summary>
    /// Class ControlReplacerSnippet.
    /// </summary>
    internal class TokenControlReplacerSnippet
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TokenControlReplacerSnippet"/> class.
        /// </summary>
        public TokenControlReplacerSnippet()
        {
            Tokens = new List<Token>();
            Controls = new Control();
        }

        /// <summary>
        /// Gets or sets the original text.
        /// </summary>
        /// <value>The original text.</value>
        public string OriginalText { get; set; }
        /// <summary>
        /// Gets or sets the tokens.
        /// </summary>
        /// <value>The tokens.</value>
        public List<Token> Tokens { get; set; }
        /// <summary>
        /// Gets or sets the controls.
        /// </summary>
        /// <value>The controls.</value>
        public Control Controls { get; set; }
    }

    /// <summary>
    /// Class ControlReplacer.
    /// </summary>
    public class TokenControlReplacer
    {
        /// <summary>
        /// The token close
        /// </summary>
        private readonly string TokenClose;
        /// <summary>
        /// The token open
        /// </summary>
        private readonly string TokenOpen;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenControlReplacer"/> class.
        /// </summary>
        /// <param name="tokenOpen">The token open.</param>
        /// <param name="tokenClose">The token close.</param>
        public TokenControlReplacer(string tokenOpen, string tokenClose)
        {
            TokenOpen = tokenOpen;
            TokenClose = tokenClose;
            Snippets = new List<TokenControlReplacerSnippet>();
        }

        /// <summary>
        /// Gets or sets the snippets.
        /// </summary>
        /// <value>The snippets.</value>
        private List<TokenControlReplacerSnippet> Snippets { get; set; }

        /// <summary>
        /// Appends the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        public void Append(string text)
        {
            if (text != "")
            {
                var snippet = new TokenControlReplacerSnippet();
                snippet.OriginalText = text;

                ExtractTokens(snippet);

                for (int i = 0; i < snippet.Tokens.Count; i++)
                {
                    Token token = snippet.Tokens[i];
                    if (i == 0)
                    {
                        snippet.Controls.Controls.Add(new LiteralControl(text.Substring(0, token.Start)));
                    }
                    else
                    {
                        snippet.Controls.Controls.Add(
                            new LiteralControl(text.Substring(snippet.Tokens[i - 1].End,
                                token.Start - snippet.Tokens[i - 1].End)));
                    }


                    snippet.Controls.Controls.Add(new PlaceHolder {ID = token.Id});
                }

                snippet.Controls.Controls.Add(
                    new LiteralControl(text.Substring(snippet.Tokens[snippet.Tokens.Count - 1].End)));


                Snippets.Add(snippet);
            }
        }

        /// <summary>
        /// Extracts the tokens.
        /// </summary>
        /// <param name="snippet">The snippet.</param>
        /// <exception cref="System.ArgumentException"></exception>
        private void ExtractTokens(TokenControlReplacerSnippet snippet)
        {
            int last = 0;
            while (last < snippet.OriginalText.Length)
            {
                // Find next token position in snippet.Text:
                int start = snippet.OriginalText.IndexOf(TokenOpen, last, StringComparison.InvariantCultureIgnoreCase);
                if (start == -1)
                    return;
                int end = snippet.OriginalText.IndexOf(TokenClose, start + TokenOpen.Length,
                    StringComparison.InvariantCultureIgnoreCase);
                if (end == -1)
                    throw new ArgumentException(string.Format("Token is opened but not closed in text \"{0}\".",
                        snippet.OriginalText));
                int eol = snippet.OriginalText.IndexOf('\n', start + TokenOpen.Length);
                if (eol != -1 && eol < end)
                {
                    last = eol + 1;
                    continue;
                }

                // Take the token from snippet.Text:
                end += TokenClose.Length;
                string token =
                    snippet.OriginalText.Substring(start, end - start).Replace(TokenOpen, "").Replace(TokenClose, "");

                var objToken = new Token
                {
                    Start = start,
                    End = end,
                    TokenName = token
                };

                if (token.Contains(":"))
                {
                    string[] nameAndAttributes = token.Split(':');
                    objToken.TokenName = nameAndAttributes[0];

                    var xml = new XmlDocument();
                    string xmlText = @"<root " + nameAndAttributes[1] + " />";
                    xml.LoadXml(xmlText);
                    if (xml.DocumentElement.Attributes != null)
                        foreach (XmlAttribute attribute in xml.DocumentElement.Attributes)
                        {
                            objToken.Attributes.Add(attribute.Name, attribute.Value);
                        }
                }

                snippet.Tokens.Add(objToken);

                last = end;
            }
        }


        /// <summary>
        /// Replaces the specified token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="text">The text.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool Replace(string token, string text)
        {
            return Replace(token, new LiteralControl(text));
        }

        /// <summary>
        /// Replaces the specified token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="control">The control.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool Replace(string token, Control control)
        {
            var tokenFound = false;
            if (token.Contains(TokenOpen) || token.Contains(TokenClose))
            {
                token = token.Replace(TokenOpen, "").Replace(TokenClose, "");
            }

            foreach (TokenControlReplacerSnippet snippet in Snippets)
            {
                IEnumerable<Token> snippetsWithToken = snippet.Tokens.Where(x => x.TokenName == token);
                if (snippetsWithToken.Any())
                {
                    tokenFound = true;
                    foreach (Token objToken in snippetsWithToken)
                    {
                        snippet.Controls.Controls.OfType<PlaceHolder>()
                            .FirstOrDefault(x => x.ID == objToken.Id)
                            .Controls.Clear();

                        if (objToken.Attributes.Count > 0)
                        {
                            foreach (var attribute in objToken.Attributes)
                            {
                                PropertyInfo property = control.GetType().GetProperty(attribute.Key);
                                if (property == null)
                                {
                                    property = control.GetType().GetProperty("Attributes");
                                    ((AttributeCollection)property.GetValue(control, null)).Add(attribute.Key, attribute.Value);
                                }
                                else
                                {
                                    property.SetValue(control, attribute.Value, null);
                                }

                            }
                        }

                        snippet.Controls.Controls.OfType<PlaceHolder>()
                            .FirstOrDefault(x => x.ID == objToken.Id)
                            .Controls.Add(control);
                    }
                }
            }

            return tokenFound;

        }

        /// <summary>
        /// Outputs this instance.
        /// </summary>
        /// <returns>Control.</returns>
        public Control Output()
        {
            var masterCtl = new Control();
            foreach (TokenControlReplacerSnippet snippet in Snippets)
            {
                masterCtl.Controls.Add(snippet.Controls);
            }

            return masterCtl;
        }
    }
}