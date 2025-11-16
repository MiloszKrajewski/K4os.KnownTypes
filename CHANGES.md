## 2.2.0 (2025/11/16)
* ADDED: KnownTypeObjectConverter to handle polymorphism for System.Text.Json serialization

## 2.1.0 (2024/07/14)
* ADDED: handles DataContact attributes as well
* CHANGED: exposing IKnownTypesRegistry and IKnownTypesResolver interfaces
* ADDED: it is possible to implement custom naming rules with IKnownTypeAliasExtractor

## 2.0.1 (2024/06/09)
* ADDED: Factory extensions methods

## 2.0.0 (2024/06/09)
* NOTE: Breaking changes! It is a successor of `K4os.Json.KnownTypes` but has been completely rewritten
* CHANGED: KnownTypesRegistry extracted to separate assembly
* CHANGED: KnownTypesSerializationBinder depends on serialization independent KnownTypesRegistry
* ADDED: KnownTypesJsonTypeInfoResolver to handle polymorphism for System.Text.Json

## 1.0.6 (2020/10/10)
* not crashing when same entry gets re-registered

## 1.0.5 (2018/07/05)
* lower required Newtonsoft.Json version to 10.0.1
* added .NET 4.5 o the mix

## 1.0.4 (2018/01/20)
* more frameworks

## 1.0.3 (2018/01/18)
* minor cleanup

## 1.0.2 (2018/01/18)
* project renamed (again)

## 1.0 (2018/01/12)
* added default fallback binder

## 0.2 (2018/01/11)
* used dictionary instead of list
* used reader-writer-lock instead of locks
* added unit tests
* .NET Standard 1.0 / .NET Framework 4.6 targets

## 0.1 (2017/11/21)
* initial release