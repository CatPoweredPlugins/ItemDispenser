# ASF Item Dispenser Plugin

# Introduction
This plugin allows you to set your bot(s) to accept incoming trade requests for certain type(s) of items from your inventory. It's primary use is giving away your coupons to people who need it. Initially I planned to make plugin for coupons only, but decided that a bit of flexibility won't hurt, so you could specify any inventory you want to give away. Also, if your bot has `AcceptDonations` set in `TradingPreferences`, trade offers are allowed, apart from items you give, to have **any** items you will receive as a donation. It **will not**, however, accept "mixed" trade offers, i.e. trades where someone wants to both exchange with you some items on 1:1 basis and at the same time take some items you dispense.

## Installation
- download .zip file from [latest release](https://github.com/Ryzhehvost/ItemDispenser/releases/latest), in most cases you need `ItemDispenser.zip`, but if you use ASF-generic-netf.zip (you really need a strong reason to do that) download `ItemDispenser-netf.zip`.
- unpack downloaded .zip file to `plugins` folder inside your ASF folder.
- (re)start ASF, you should get a message indicating that plugin loaded successfully. 

## Configuration
For this plugin to work you need to add a specific configuration parameter to configuration of your bot. Example of general structure of configuration parameter is shown below:
```
"Ryzhehvost.DispenseItems" : [
                              { "AppID": 753, "ContextID": 3},
                              { "AppID": 753, "ContextID": 6, "Types": [ 2, 4 ]},
                              { "AppID": 440, "ContextID": 2}
                             ]
```
This is an example structure, **don't use it on real bots**, you may lose items that you still need. In this structure:

`Ryzhehvost.DispenseItems` - is parameter of **array of objects** type. Every element of array specifies steam inventory and optionally type of an item to give away. In this element there are up to three fields:

`AppID` - field of **uint** type. AppID of application which inventory we want to give away. This field is required.<br/>
`ContextID` - field of **ulong** type. ContextID of inventory we want to give away. This field is required.<br/>
`Types` - field of **array of bytes** type. This field is optional, and only makes sense if you plan to give away items from steam "Community" inventory. Applicable types are the same as in ASF option [MatchableTypes](https://github.com/JustArchiNET/ArchiSteamFarm/wiki/Configuration#matchabletypes).

So, with the example above, plugin will accept trades with requests of such items:
1. `{ "AppID": 753, "ContextID": 3}` steam coupons 
2. `{ "AppID": 753, "ContextID": 6, "Types": [ 2, 4 ]}` emoticons and profile backgrounds
3. `{ "AppID": 440, "ContextID": 2}` any items from Team Fortress 2 inventory

If you only want to give away coupons (and that's a primary use of this plugin), you just need to add to configuration of your bot following structure (you can just copy & paste):

```
"Ryzhehvost.DispenseItems" : [{ "AppID": 753, "ContextID": 3}]
```

### How to find AppID and ContextID?
It's simple, as one-two-three:
1. Open inventory that you plan to give away in browser, and select any item.
2. Copy the link to that item and copy it to notepad, **or**, if your browser shows you the link you're hovering, just look at it.
3. Link will have three numbers after `#` character, separated with `_`. You need first two, those are AppID and ContextID respectively. Look at the following picture for better understanding:
![AppID and ContextID](https://i.imgur.com/hom2PZq.png)

---

# Плагин ASF для раздачи предметов


# Введение
Этот плагин позволяет вам настроить бота так, чтобы он принимал входящие обмены с запросом вещей определённых типов из вашего инвентаря. Его основное назначение - раздача купонов нуждающимся в них людях. Изначально я планировал создать плагин исключительно для купонов, но решил что немного гибкости не повредит, поэтому вы можете его настроить на раздачу предметов из любого инвентаря стим. Также, если у вашего бота включено значение `AcceptDonations` в параметре `TradingPreferences`, в предложениях обмена, кроме вещей которые вы отдаёте, могут присутствовать также **любые** предметы которые вы получите в качестве пожертвований. Однако он **не** примет "смешанные" предложения обмена, т.е. такие где с вами хотят обменяться предметами 1:1 и одновременно взять предметы которые вы раздаёте.

## Установка
- скачайте файл .zip из [последнего релиза](https://github.com/Ryzhehvost/ItemDispenser/releases/latest), в большинстве случаев вам нужен файл `ItemDispenser.zip`, не если вы по какой-то причине пользуетесь ASF-generic-netf.zip (а для этого нужны веские причины) - скачайте `ItemDispenser-netf.zip`.
- распакуйте скачанный файл .zip в папку `plugins` внутри вашей папки с ASF.
- (пере)запустите ASF, вы должны получить сообщение что плагин успешно загружен. 

## Конфигурирование
Для работы этого плагина вам необходимо добавить особый конфигурационный параметр в конфигурацию вашего бота. Пример общего вида структуры этого конфигурационного параметра показан ниже:
```
"Ryzhehvost.DispenseItems" : [
                              { "AppID": 753, "ContextID": 3},
                              { "AppID": 753, "ContextID": 6, "Types": [ 2, 4 ]},
                              { "AppID": 440, "ContextID": 2}
                             ]
```
Это только пример структуры, **не используйте его на реальных ботах**, вы можете потерять нужные вам предметы. В этой структуре:

`Ryzhehvost.DispenseItems` - параметр типа **массив объектов**. Каждый элемент этого массива задаёт один из инвентарей Steam, из которого вы хотите раздавать предметы. В этом элементе может быть до трёх полей:

`AppID` - поле типа **uint**. AppID приложения, из инвентаря которого вы хотите раздавать предметы. Это обязательное поле.<br/>
`ContextID` - поле типа **ulong**. ContextID инвентаря, из которого вы хотите раздавать предметы. Это обязательное поле.<br/>
`Types` - поле типа **массив byte**. Это необязательное поле, и оно имеет смысл только если вы планируете раздавать предметы из инвентаря Steam "Сообщество". Возможные типы такие же, как в параметре бота [MatchableTypes](https://github.com/JustArchiNET/ArchiSteamFarm/wiki/Configuration-ru-RU#matchabletypes) в ASF.

Итак, если мы используем структуру из примера выше, плагин будет принимать предложения обмена с запросом следующих предметов:
1. `{ "AppID": 753, "ContextID": 3}` купоны Steam 
2. `{ "AppID": 753, "ContextID": 6, "Types": [ 2, 4 ]}` смайлики и фоны профиля Steam
3. `{ "AppID": 440, "ContextID": 2}` любые предметы из инвентаря Team Fortress 2

Если вы хотите только раздавать купоны (а это и есть основное применение этого плагина), вам нужно просто добавить следующую структуру в конфигурацию вашего бота (можно просто скоприровать и вставить):

```
"Ryzhehvost.DispenseItems" : [{ "AppID": 753, "ContextID": 3}]
```

### Как найти AppID и ContextID?
Это очень просто, как раз-два-три:
1. Откройте инвентарь, предметы из которого хотите раздать, в браузере, и выберите любой предмет.
2. Скопируйте ссылку на этот предмет в блокнот, **или**, если ваш браузер показывает ссылку, на которую вы наводите, просто посмотрите на неё.
3. Ссылка будет иметь три числа после символа `#`, разделённые символом `_`. Вам нужны первые два, это и будут AppID и ContextID соответственно. Посмотрите на следующую картинку для лучшего понимания:
![AppID и ContextID](https://i.imgur.com/hom2PZq.png)

![downloads](https://img.shields.io/github/downloads/Ryzhehvost/ItemDispenser/total.svg?style=social)
