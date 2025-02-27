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
