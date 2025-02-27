# Ordnerstruktur

Die höchste Ebenne der Ordnerstruktur ist die Aufteilung nach folgenden Ordnern:

ComponentsUI
Components
-> Hier wird der Business-Code definiert mit der Aufteilung nach UI und non UI. 
   Die Unterordner sollten den Business-Domänen folgen. Domain-Driven.

   Da WordCountParser in der Aufgabe stand wäre dies ein Business-Fall.
   
GenericComponents
GenericComponentsUI
-> Hier wird der Non-Business-Code definiert mit der Aufteilung nach UI und non UI. 
   In etwa wie ein Utils-Ordner.

   Der FileLoader könnte auch universel in anderen Projekten verwendet werden. 
   Somit kein Business-Fall.

Ressources
-> Images, Translations, etc.

# Code-Design

- Die Interfaces wurden mit Dependency Injection verwendet nach dem SOLID-Prinzip : Open/Closed Principle definiert.
- Die Klassen sind atomar gehalten nach dem SOLID-Prinzip: Single Responsibility Principle
- Dependency Injection wurde für SOLID-Prinzip "Dependency Inversion Principle" angewandt
- Interfaces folgen nur einem Topic: Interface Segregation Principle.

# MVVM

Die WordCounterForm wurde mit dem MVVM-Pattern entwickelt. Somit hat die View noch ein
Model und einen State. Durch Command-Pattern kann die View das Model anrufen.
Somit wird sicher gestellt, dass in der View nur View-Code existiert und keine Model-Logik.

# Install.bat

Um die Install.bat benutzen zu können muss die nuget.exe in das Verzeichis gelegt werden.
Dies installiert dann automatisch alle Nuget-Abhängigkeiten.

# .net 8

.net 8 braucht ein nuget compatibilitäts packet, so dass WPS-Forms programmiert werden können.
Daher die Nuget-Abhängigkeit.

# Benamungsregeln

C# in Visual Studio stellt bei den Benamungsregeln eine Ausnahme zu den meisten 
Programmiersprachen. Ehrlich gesagt habe ich mir das Benamungsschema nie wirklich zu herzen 
genommen. Wenn es einen Vorgegeben Code-Style gibt schau ich mir diesen natürlich gerne an.

# sonstiges

Visual Studio 2022 Community Edition wurde verwendet.

