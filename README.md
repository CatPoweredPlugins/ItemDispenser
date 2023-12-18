# ASF Item Dispenser Plugin

# Introduction
This plugin allows you to set your bot(s) to accept incoming trade requests for certain type(s) of items from your inventory. It's primary use is giving away your coupons to people who need it. Initially I planned to make plugin for coupons only, but decided that a bit of flexibility won't hurt, so you could specify any inventory you want to give away. Also, if your bot has `AcceptDonations` set in `TradingPreferences`, trade offers are allowed, apart from items you give, to have **any** items you will receive as a donation. It **will not**, however, accept "mixed" trade offers, i.e. trades where someone wants to both exchange with you some items on 1:1 basis and at the same time take some items you dispense.

## Installation
- download `ItemDispenser.zip` file from [latest release](https://github.com/Rudokhvist/ItemDispenser/releases/latest).
- unpack downloaded .zip file to `plugins` folder inside your ASF folder.
- (re)start ASF, you should get a message indicating that plugin loaded successfully. 

## Configuration
For this plugin to work you need to add a specific configuration parameter to configuration of your bot. Example of general structure of configuration parameter is shown below:
```
"Rudokhvist.DispenseItems" : [
                              { "AppID": 753, "ContextID": 3},
                              { "AppID": 753, "ContextID": 6, "Types": [ 2, 4 ]},
                              { "AppID": 440, "ContextID": 2}
                             ]
```
This is an example structure, **don't use it on real bots**, you may lose items that you still need. In this structure:

`Rudokhvist.DispenseItems` - is parameter of **array of objects** type. Every element of array specifies steam inventory and optionally type of an item to give away. In this element there are up to three fields:

`AppID` - field of **uint** type. AppID of application which inventory we want to give away. This field is required.<br/>
`ContextID` - field of **ulong** type. ContextID of inventory we want to give away. This field is required.<br/>
`Types` - field of **array of bytes** type. This field is optional, and only makes sense if you plan to give away items from steam "Community" inventory. Applicable types are the same as in ASF option [MatchableTypes](https://github.com/JustArchiNET/ArchiSteamFarm/wiki/Configuration#matchabletypes).

So, with the example above, plugin will accept trades with requests of such items:
1. `{ "AppID": 753, "ContextID": 3}` steam coupons 
2. `{ "AppID": 753, "ContextID": 6, "Types": [ 2, 4 ]}` emoticons and profile backgrounds
3. `{ "AppID": 440, "ContextID": 2}` any items from Team Fortress 2 inventory

If you only want to give away coupons (and that's a primary use of this plugin), you just need to add to configuration of your bot following structure (you can just copy & paste):

```
"Rudokhvist.DispenseItems" : [{ "AppID": 753, "ContextID": 3}]
```

### How to find AppID and ContextID?
It's simple, as one-two-three:
1. Open inventory that you plan to give away in browser, and select any item.
2. Copy the link to that item and copy it to notepad, **or**, if your browser shows you the link you're hovering, just look at it.
3. Link will have three numbers after `#` character, separated with `_`. You need first two, those are AppID and ContextID respectively. Look at the following picture for better understanding:
![AppID and ContextID](https://i.imgur.com/hom2PZq.png)

![downloads](https://img.shields.io/github/downloads/Rudokhvist/ItemDispenser/total.svg?style=social)
