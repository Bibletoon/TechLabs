# Лабораторная работа №1. Hello world

## Задание 1

Изучить механизм интеропа между языками, попробовать у себя вызывать C/C++ (Не C++/CLI) код (суммы чисел достаточно) из Java и C#. В отчёте описать логику работы, сложности и ограничения этих механизмов

### Часть 1. C++ interop from C#

Было создано три файла:

* UnitEntryPoint.cpp - Входная точка будущей DLL
* UnitFunctions.h - Заголовочный файл с объявлением функций
* UnitFunctions.cpp - Реализация функций из предыдущего файла

Затем с помощью компилятора g++ происходит создание DLL

```bash
g++ -c UnitFunctions.cpp
g++ -c UnitEntryPoint.cpp
g++ -o Functions.dll UnitEntryPoint.o UnitFunctions.o
```

В C# классе подключение метода из DLL происходит с помощью атрибута DllImport

```cs
[DllImport("Functions.dll")]
static extern int SumTwoIntsInterop(int a, int b);
```

Этот метод можно уже полноценно использовать в программе

```cs
Console.WriteLine(SumTwoIntsInterop(5, 10));
```

### Часть 2. C++ interop from Java

В Java есть инструмент для запуска С/C++ кода - JNI

Чтобы его использовать необходимо сначала написать класс, который в будущем будет фасадом для C++ кода

```java
public class JNIMethods {
    native int SumTwoIntsInterop(int a, int b);
}
```

Затем необходимо создать заголовочный файл, сделать это можно инструментами SDK

```bash
javac -h . JNIMethods.java
```

Либо написать самому по шаблону, если привередливые инструменты отказываются работать

Далее под заголовочный файл пишется реализация и с помощью g++ собирается в динамическую библиотеку

```
g++ -I"$JAVA_HOME/include" -I"$JAVA_HOME/include/linux" -fPIC JNISum.cpp -shared -o jnisum.so -Wl,-soname -Wl,--no-whole-archive
```

Теперь в уже написанном нами классе можно подключить созданную библиотеку

```java
public class JNIMethods {
    native int SumTwoIntsInterop(int a, int b);
    static {
        System.load(System.getProperty("user.dir")+"/libs/jnisum.so");
    }
}
```

Этот класс уже можно полноценно использовать в программе

### Часть 3. Результаты и выводы

Механизмы интеропа между языками позволяют писать модули программы на наиболее пригодных для этого языках, а затем использовать их вместе.

Платформы .Net и JVM позволяют подключать динамические библиотеки с помощью Api операционной системы

Это создаёт ряд ограничений:

* Платформозависимость - динамические библиотеки нужно собирать отдельно под отдельные платформы и обеспечивать механизм подгрузки нужной библиотеки в зависимости от платформы, на которой исполняется приложение
* Сложность отладкии
* Частая пересборка при изменениях - После смены сигнатуры или названия метода с одной из сторон необходимо полностью пересобирать динамическую библиотеку. В JNI это необходимо делать ещё и при смене названия класса, к которому библиотека подключается, или пакета где он находится

## Задание 2

Написать немного кода на Scala и F# с использованием уникальных возможностей языка - Pipe operator, Discriminated Union, Computation expressions и т.д. . Вызвать написанный код из обычных соответствующих ООП языков (Java и С#) и посмотреть во что превращается написанный раннее код после декомпиляции в них.

### Часть 1. F# to C#

Для примера был написан небольшой кусок кода для маппинга Dto в сущность с валидацией на основе аппликативов

```fs
module FsProgram

open System
open System.Text.RegularExpressions

module Result =
    let apply s f = match f, s with
        | Ok f, Ok s -> Ok (f s)
        | Ok f, Error s -> Error s
        | Error f, Ok s -> Error f
        | Error f, Error s -> Error (f @ s)
    let pure = Ok

let (|Regex|_|) pattern input =
    let m = Regex.Match(input, pattern)
    if m.Success then seq m.Groups |> Seq.map (fun g -> g.Value) |> Seq.toList |>  List.tail |> Some
    else None

type Country =
    | Russia
    | USA
    | Canada

type UserDto = {
    Username : string
    Name : string
    Country : Country
}

type User = {
    Username : string
    FirstName : string
    LastName : string
    Country : Country
}

module User =
    let FirstName (user : User ) = user.FirstName

let validateName name =
    match name with
    | Regex "(^[A-Z]{1}[a-z]*) ([A-Z]{1}[a-z]*)$" [firstName; lastName] -> Ok (firstName, lastName)
    | _ -> Error "Name has wrong format"

let validateUsername username =
    match username with
    | Regex "^[A-Za-z0-9]+$" _ -> Ok username
    | _ -> Error "Username has wrong format"

let createUserFromDto userDto =
    let create (firstName, lastName) username country = {
        Username = username
        FirstName = firstName
        LastName = lastName
        Country = country}

    Ok create
    |> Result.apply (validateName userDto.Name |> Result.mapError List.singleton)
    |> Result.apply (validateUsername userDto.Username |> Result.mapError List.singleton)
    |> Result.apply (Result.Ok userDto.Country)

let dto = {
    Username = "bibletoon"
    Name = "Aboba Kekw"
    Country = Russia
}

match dto |> createUserFromDto |> Result.map User.FirstName |> Result.mapError (fun l ->  String.Join(",", l)) with
    | Ok res -> printfn $"Success %s{res}"
    | Error err -> printfn $"Error %s{err}"
```

При [декомпиляции кода в C#](https://sharplab.io/#v2:DYLgZgzgNAJiDUAfAtgexgV2AUwAQDEIAFAJ1QHMSBDZAWACgHUAHbAO1wGUBPCAF2x16Ldl14DkAOgAq2AB59JAJWzksVEgFE5zEtggQAlqjYQGDNJhy4VELH1wBeBrle4cDqs2bBuuCLhgTrjIVHwAxgAWgVD+uADuhnyRLm5piLgA8gDWMVm5AQC0AHz5uAAUQRAAlKlprhk5eZokZCRxJbgtbf519RndqO1gsU1FpYPDfeldrUPNc+3jsz2VuAAC6/619GkeuMwYesE55vT75Ygq5PKIAPqI1QdhAiQchmyHDs67bvvIwWu8kkAFkwlFyh8vrFmC9sG8dmlDEEpJwMOFwvoAslRBBsABHEKSADiZAwzACiFKnAJklCzAqYAwHHIuE65EkADUqMAMNgnlSuLS+KgADKGfi4QW4cX8SR8KiGYBS6moZDYPrYYB43AAORMGsY9D43FYuAAwqhmXwSH4fjMlBgDIYqH0MgBVTgAQTdFqobCoMFdRpNZvdeJIABERcEAN59cPwgPq3Agfw2j7kPq6mh4NP8EiZvqW6221MWq1sG3cBgAXzOobwifajlw8d+rmbybz6cLbCzHYIhhI/BzKfzGf7fVFVFHufLBaLg5LVbLaZX1brZ0sWCbEacfX2+GHc5T5Sd8PLzdwT1bF5IkmPI74Y8NDH2ADceYYgwJX7huwPQdQgiaJAMSZJfSBORcAAInKAA9ABtL1CgALQAXVjABGWskKoQoAC8MIAKiecoUPQrDcPwojSOqAASWDcCQsATxfXMAG53FnDj1QwtlSiaSp2NfWJgF419ETcDI7kElZ5lg/9IlnBIyH7QIhhA2Czk/b9f2wLt53vQD7TcEColwEz5wglJBwyaC4OQ1C0IIwiAAZCgATgw+AmNwOTOiaaz1V9QKJkWOCjJTFSAnidTWTALSwh0o19nCPQwkMiN8DIZBo1QKyIwKoC9mwBwMuwLLGVE3NxMk3MnhCvBwkras42mNxorwO8I27TrXCfU8esCWrQsHNIZ2G4IJOGgaK1LO1cFaxb6yNNImkqrK3VKWx7EkLwfD8cov2AH8sv/e8Cskf9BT24BFHpSYZQlRQjH7DwTGkhpdv0fbDt8CpTvOgRuqK+FrrBu6/oeukvGe2U3szT62G+lUbBhxQAeO+7FGC4qRUkDdbR2d9ytwGAY1bds0jB1tYIAI0MBnPpMVK0n/emvQZ1AGaoXAAGlsGyeJ2bcYmlsdZ1gzWixwWiSnCsFLbQZyvKSuhuxYfpXBm0fMa8E1/ansiypmXceSuEnDkAClUA+cpYKgJ33GqJ5bN9Jo9GWXQPj4MAOCYtEMSxXAAFIIFjb3azFhoFPaeF2k6X2qwD3AmOeiPY0TmOgA=) можно посмотреть, во что тот превращается.

**Disctiminated Union** 

Если кейсы DU не хранят в себе значени какого-то типа, то он превращается в запечатанный класс, внутри которого находится статический класс Tags, отражающий все значения DU. Сам же класс содержит флаги для проверки на соответсвие какому-либо из значений и методы сравнения.

Если кейсы хранят в себе значение какого-то типа, то он превращается в абстрактный класс со всё тем же статическим классом Tags и методами сравнения, но теперь все кейсы представляют собой отдельные классы-наследники и содержат внутри себя значение заданного типа

**Pipe operator**

Pipe оператор просто превращается в последовательный вызов функций

**Active pattern matching**

Функция для Active pattern matching просто вызывается во время проверок внутри pattern matching

**Pattern matching**

Pattern matching превращается в проверку различных case-ов через if/else

### Часть 2. Scala to Java

Для просмотра функциональных возможностий Scala был написан небольшой кусок кода

```scala
package com.bibletoon

import scala.util.chaining.scalaUtilChainingOps;

sealed trait Identifier;
case class Username(firstName : String, secondName: String) extends Identifier;
case class Email(value : String) extends Identifier;

object Scala {
  def checkUser(id: Identifier) = id match {
    case Username(firstName, secondName) => Left(secondName+" "+firstName)
    case Email(email) => Right(email.reverse)
  }

  def main(args: Array[String]) = {
    checkUser(Username("aboba", "abobievich")) match {
      case Left(value) => value.pipe(println).pipe(println)
      case Right(value) => println(value)
    }
  }
}
```

С помощью декомпиляции .class файлов в Java можно посмотреть во что эти возможности превращаются

**Trait**

Trait превращается в Java-интерфейс

**Case class**

Case class - подобие record из .Net мира, иммутабельный класс имеющий методы сравнения, копирования а так же композиции

**Pattern matching**

Pattern matching первращается в набор if/else с проверкой через isinstanceof

**Pipe**

Pipe не превращается в другую конструкцию, а просто импортируется из scala-модуля

## Задание 3

Написать алгоритм обхода графа (DFS и BFS) на языке Java, собрать в пакет и опубликовать (хоть в Maven, хоть в Gradle, не имеет значения). Использовать в другом проекте на Java/Scala этот пакет. Повторить это с C#/F#. В отчёте написать про алгоритм работы пакетных менеджеров, особенности их работы в C# и Java мирах.

### Часть 1. .Net мир

Чтобы собрать свою в библиотеку необходимо 

* В IDE в настройках проекта установить нужные параметры в спецификации, затем нажать пкм на проект и выбрать опцию "pack" либо установить в настройках "Create nuget on build"
* В консоле ввести команду ```nuget spec ProjectName.cs```, настроить спецификацию пакета а затем ввести команду ```nuget pack```

Полученый .nupkg файл можно как подключать локально, так и загрузить в любой из репозиториев пакетов (самый известный и общепринятый - nuget.org)

### Часть 2. Java мир

В Java есть несколько инструментов для работы с зависимостями и публикации пакетов. Мною был опробован Gradle

Для конфигурации работы Gradle используются файлы settings.gradle и build.gradle

Файл build.gradle выглядит так:

```gradle
plugins {
    id 'java'
}

group 'com.bibletoon'
version '1.0-SNAPSHOT'

repositories {
    mavenCentral()
}

dependencies {
}

test {
    useJUnitPlatform()
}
```

Чтобы добавить возможность сборки проекта в jar файл нужно добавить раздел

```gradle
jar {
    manifest {
        attributes "Main-Class": "com.bibletoon.graphAlgorithms.Algorithms"
    }

    from {
        configurations.runtimeClasspath.collect { it.isDirectory() ? it : zipTree(it) }
    }
}
```

Чтобы добавить возможность публикации проекта в локальный Maven репозиторий нужно добавить плагин maven-publish

```gradle
plugins {
    id 'java'
    id 'maven-publish'
}
```

Чтобы добавить возможность публикации в удалённый Maven репозиторий нужно добавить блок

```gradle
publishing {
    publications {
    }
    repositories {
        maven {
            name = "MyRepo"
            repository name
            url = "http://my.org.server/repo/url"
            credentials {
                username = 'alice'
                password = 'my-password'
            }
        }
    }
}
```

### Часть 3. Сравнение

В .Net мире управление пакетами централизованное и единое, что упращает использование, в то время как менеджеры пакетов в Java сложнее в использовании, но позволяют производить более гибкую настройку

## Задание 4

Изучить инструменты для оценки производительности в C# и Java. Написать несколько алгоритмов сортировок (и взять стандартную) и запустить бенчмарки (в бенчмарках помимо времени выполнения проверить аллокации памяти). В отчёт написать про инструменты для бенчмаркинга, их особености, анализ результатов проверок.

### Часть 1. Бенчмарки в .Net

Самым распространённым и удобным инструментом для бенчмаркинга в .Net является пакет Benchmark.Net

Класс с бенчмарком выглядит так

```cs
using BenchmarkDotNet.Attributes;

namespace CsSortBenchmark;

[MemoryDiagnoser]
public class Benchmark
{
    private int[] _arrayForDefaultSort;
    private int[] _arrayForMergeSort;
    
    [IterationSetup]
    public void Setup()
    {
        _arrayForDefaultSort = new int[2000000];
        _arrayForMergeSort = new int[2000000];
        Random r = new Random();

        for (int i = 0; i < 2000000; i++)
        {
            _arrayForDefaultSort[i] = r.Next();
            _arrayForMergeSort[i] = r.Next();
        }
    }

    [Benchmark]
    public void DefaultSort()
    {
        Array.Sort(_arrayForDefaultSort);
    }

    [Benchmark]
    public void MergeSort()
    {
        _arrayForMergeSort = MergeSortAlgorithm.MergeSort(_arrayForMergeSort);
    }
}
```

Запуск бенчмарка проихсодит так

```cs
BenchmarkRunner.Run<Benchmark>();
```

Результаты

|      Method |     Mean |   Error |   StdDev |       Gen 0 |      Gen 1 |     Gen 2 |     Allocated |
|------------ |---------:|--------:|---------:|------------:|-----------:|----------:|--------------:|
| DefaultSort | 163.9 ms | 3.24 ms |  8.43 ms |           - |          - |         - |         336 B |
|   MergeSort | 461.0 ms | 9.18 ms | 25.73 ms | 102000.0000 | 37000.0000 | 5000.0000 | 771,190,440 B |

### Часть 2. Бенчмарки в Java

