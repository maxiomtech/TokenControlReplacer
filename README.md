TokenControlReplacer
===============

Class library to take a string input with tokens and replace with LiteralControls and UserControls

## Features ##

* This utility library allows you to take multiple text strings containg [TOKENS] and replace them with anything that derives from the <code>Control</code> type. 
* Any additional non-token text surrounding your tokens will be converted into a <code>LiteralControl</code>
* Attributes are also supported for tokens. __[BUTTON:Text="Click Me"]__ will replace the Text property on a button control. Any attribute added that is not a valid property will be added to the controls attribute collection.
* Token locations are processed only once and are cataloged a preformance benefit on additional replacements.


## Instructions ##

* Add this project to your solution or simple copy the <code>ControlReplacer.cs</code> class into your project.
* Initialize the class and assign what begin and ends your tokens
```C#
var replacer = new TokenControlReplacer("[", "]");
```
* Append one or more text blocks to the class
```C#
replacer.Append("I am a block of text with a [BUTTON] token.");
```
* Identity the token and what you would like to replace it with.
```C#
replacer.Replace("[BUTTON]", new Func<Control>(() =>
{
    var buttonCtl = new LinkButton();
    buttonCtl.Click += delegate(object o, EventArgs args) { Response.Redirect("http://inspectorit.com"); };
    buttonCtl.Text = "Go to Website";
    return buttonCtl;
})());
```
* Another replacement example
```C#
replacer.Replace("[BUTTON]", LoadControl("/PathToControl.ascx"));
```
* Add the replaced controls to another control. Recommend a <code>PlaceHolder</code> control
```C#
placeHolderOutput.Controls.Add(replacer.Output());
```