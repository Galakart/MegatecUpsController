
# Megatec UPS Controller
Приложение для контроля вашего ИБП на основе Megatec-протокола (Ippon и некоторые другие), подключаемого к компьютеру по USB, а также для управления питанием компьютера.

![Главное окно](/img/main.png "Главное окно")

## История создания или зачем оно вообще сделано
После того как дома уже третий день подряд вырубили электричество, терпение подошло к концу, и в ближайшем ДНСе был куплен мой первый ИБП IPPON Back Basic 1050 с возможностью подключения по USB.

Однако тут же выяснилось, что родное приложение Иппона написано на Java и являет собой ~~тормозное убожество~~ не очень удобную программу. Да, функциональная, с возможностью контроля по сети, но мне это всё было не нужно, хотелось красивый вывод всех показателей ИБП, выключение по событиям, да и всё на этом.

Конечно есть аналоги, например Energy Controller 2. Намного функциональнее родной, всё выводит красиво. Но всё же есть пара надоедающих багов, вроде улетания главного окна за пределы экрана при загрузке компа.

**Так зачем оно всё таки создано, при наличии кучи более функциональных аналогов?**  
Всё просто - захотелось сделать стильно-модно-молодёжно-масштабируемо, сделать СВОЁ, и самое главное - поупражняться в C# (да, первый проект на сишарпе, после многих лет работы на java и python).  
Да, тут нет многого из того, что есть даже в родной версии. Но... если проект не будет заброшен, то всё реализуемо.

## Требования к ОС
- Windows 7 и выше
- NET Framework 4.7.2 и выше

## Как начать это использовать?
- Последнюю версию в виде zip-архива можно скачать по ссылке [https://github.com/Galakart/MegatecUpsController/releases/latest](https://github.com/Galakart/MegatecUpsController/releases/latest "ЗДЕСЬ")
- Справочная система доступна здесь же на [Github Wiki](https://github.com/Galakart/MegatecUpsController/wiki "Github Wiki")
- Эволюция (она же История версий) полностью описана в файле [CHANGELOG.md](https://github.com/Galakart/MegatecUpsController/blob/master/CHANGELOG.md "CHANGELOG.md")

## Что оно умеет
+ Красивое отображение получаемых от ИБП данных:
	+ входное и выходное напряжение
	+ напряжение и заряд батареи
	+ частота
	+ ток нагрузки ИБП (в процентах, амперах, ваттах, вольт-амперах)
	+ температура ИБП (хотя в большинстве случаев она всегда 25°)
	+ состояние AVR
+ Масштабируемый интерфейс (хоть на весь экран)
+ Возможность включать-выключать надоедливую пищалку ИБП
+ График входного и выходного напряжения
+ Выключение или гибернация компьютера при разряде батареи
+ Настройка напряжений батареи и мощности под ваш ИБП
+ Запись событий в текстовый файл
+ Отправка команды по SSH на удалённую Linux-машину, при завершении работы/гибернации

![Окно настроек](/img/settings.png "Окно настроек")

## На чём же всё это построено
- C# WPF в Microsoft Visual Studio 2019
- внешний вид интерфейса - по мотивам программы [Energy Controller 2](https://sites.google.com/site/ibakhlab/News/energycontroller20582332200sp5 "Energy Controller 2")
- библиотека UsbLibrary.dll - доработанная от [adelectronics.ru](https://adelectronics.ru/2016/11/22/usblibrary-c-usb-hid-library/ "adelectronics.ru") (так как [изначальная](https://www.codeproject.com/Articles/18099/A-USB-HID-Component-for-C "изначальная") попила кровушки и блистала багами вроде невозможности видеть потерю связи с USB)
- описание Megatec-протокола - от [networkupstools.org](https://networkupstools.org/protocols/megatec.html "networkupstools.org")
- немногочисленные иконки - [FontAwesome](https://fontawesome.com/ "FontAwesome") (хотя тащить всю библиотеку ради пары иконок такое себе. Зато они масштабируются)
- вывод графика - библиотека [InteractiveDataDisplay.WPF](https://github.com/microsoft/InteractiveDataDisplay.WPF "InteractiveDataDisplay.WPF")
- работа с SSH - библиотека [SSH.NET](https://github.com/sshnet/SSH.NET "SSH.NET")

## Про лицензию
Для тех кто не осилил перевод лицензии Apache License 2.0, её краткая неполная суть:
- программа бесплатная
- можно распространять и изменять
- если распространили или изменили - укажите что поменяли и первоначального автора (меня :-) )
- программа поставляется как есть, автор не даёт никаких гарантий безошибочной работы, не оказывает техническую поддержку, не несёт никакой ответственности за потерянные данные, сгоревшие компы, взорванные ИБП и тому подобное.

**Однако вы всегда можете скинуться автору на печеньки:**  
BTC: bc1q5aptd289qsvrtsf9t2z42udda5t70e7hc39sc2

Удачи и бесперебойной работы!

Copyright © 2020 Артем Галактионов
galakart.android@gmail.com
