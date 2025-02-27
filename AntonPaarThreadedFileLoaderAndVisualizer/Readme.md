# Ordnerstruktur

Die h�chste Ebenne der Ordnerstruktur ist die Aufteilung nach folgenden Ordnern:

ComponentsUI
Components
-> Hier wird der Business-Code definiert mit der Aufteilung nach UI und non UI. 
   Die Unterordner sollten den Business-Dom�nen folgen. Domain-Driven.

   Da WordCountParser in der Aufgabe stand w�re dies ein Business-Fall.
   
GenericComponents
GenericComponentsUI
-> Hier wird der Non-Business-Code definiert mit der Aufteilung nach UI und non UI. 
   In etwa wie ein Utils-Ordner.

   Der FileLoader k�nnte auch universel in anderen Projekten verwendet werden. 
   Somit kein Business-Fall.

Ressources
-> Images, Translations, etc.

# Code-Design

- Die Interfaces wurden mit Dependency Injection verwendet nach dem SOLID-Prinzip : Open/Closed Principle definiert.
- Die Klassen sind atomar gehalten nach dem SOLID-Prinzip: Single Responsibility Principle
- Dependency Injection wurde f�r SOLID-Prinzip "Dependency Inversion Principle" angewandt
- Interfaces folgen nur einem Topic: Interface Segregation Principle.

# MVVM

Die WordCounterForm wurde mit dem MVVM-Pattern entwickelt. Somit hat die View noch ein
Model und einen State. Durch Command-Pattern kann die View das Model anrufen.
Somit wird sicher gestellt, dass in der View nur View-Code existiert und keine Model-Logik.

# Install.bat

Um die Install.bat benutzen zu k�nnen muss die nuget.exe in das Verzeichis gelegt werden.
Dies installiert dann automatisch alle Nuget-Abh�ngigkeiten.

# .net 8

.net 8 braucht ein nuget compatibilit�ts packet, so dass WPS-Forms programmiert werden k�nnen.
Daher die Nuget-Abh�ngigkeit.

# Benamungsregeln

C# in Visual Studio stellt bei den Benamungsregeln eine Ausnahme zu den meisten 
Programmiersprachen. Ehrlich gesagt habe ich mir das Benamungsschema nie wirklich zu herzen 
genommen. Wenn es einen Vorgegeben Code-Style gibt schau ich mir diesen nat�rlich gerne an.

# sonstiges

Visual Studio 2022 Community Edition wurde verwendet.

