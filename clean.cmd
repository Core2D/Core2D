@echo off

rmdir /Q /S dependencies\netDxf\netDxf\bin
rmdir /Q /S dependencies\netDxf\netDxf\obj

rmdir /Q /S dependencies\netDxf\TestDxfDocument\bin
rmdir /Q /S dependencies\netDxf\TestDxfDocument\obj

rmdir /Q /S dependencies\FileWriter.Dxf\bin
rmdir /Q /S dependencies\FileWriter.Dxf\obj

rmdir /Q /S dependencies\FileWriter.Emf\bin
rmdir /Q /S dependencies\FileWriter.Emf\obj

rmdir /Q /S dependencies\FileWriter.Pdf-core\bin
rmdir /Q /S dependencies\FileWriter.Pdf-core\obj

rmdir /Q /S dependencies\FileWriter.Pdf-wpf\bin
rmdir /Q /S dependencies\FileWriter.Pdf-wpf\obj

rmdir /Q /S dependencies\Log.Trace\bin
rmdir /Q /S dependencies\Log.Trace\obj

rmdir /Q /S dependencies\Renderer.Dxf\bin
rmdir /Q /S dependencies\Renderer.Dxf\obj

rmdir /Q /S dependencies\Renderer.PdfSharp-core\bin
rmdir /Q /S dependencies\Renderer.PdfSharp-core\obj

rmdir /Q /S dependencies\Renderer.PdfSharp-wpf\bin
rmdir /Q /S dependencies\Renderer.PdfSharp-wpf\obj

rmdir /Q /S dependencies\Renderer.Perspex\bin
rmdir /Q /S dependencies\Renderer.Perspex\obj

rmdir /Q /S dependencies\Renderer.WinForms\bin
rmdir /Q /S dependencies\Renderer.WinForms\obj

rmdir /Q /S dependencies\Renderer.Wpf\bin
rmdir /Q /S dependencies\Renderer.Wpf\obj

rmdir /Q /S dependencies\Serializer.Newtonsoft\bin
rmdir /Q /S dependencies\Serializer.Newtonsoft\obj

rmdir /Q /S dependencies\Serializer.ProtoBuf\bin
rmdir /Q /S dependencies\Serializer.ProtoBuf\obj

rmdir /Q /S dependencies\Serializer.ProtoBuf.Generate\bin
rmdir /Q /S dependencies\Serializer.ProtoBuf.Generate\obj

rmdir /Q /S dependencies\Serializer.Xaml\bin
rmdir /Q /S dependencies\Serializer.Xaml\obj

rmdir /Q /S dependencies\TextFieldReader.CsvHelper\bin
rmdir /Q /S dependencies\TextFieldReader.CsvHelper\obj

rmdir /Q /S dependencies\TextFieldWriter.CsvHelper\bin
rmdir /Q /S dependencies\TextFieldWriter.CsvHelper\obj

rmdir /Q /S tests\Core2D.UnitTests\bin
rmdir /Q /S tests\Core2D.UnitTests\obj

rmdir /Q /S tests\Core2D.Perspex.UnitTests\bin
rmdir /Q /S tests\Core2D.Perspex.UnitTests\obj

rmdir /Q /S tests\Core2D.Wpf.UnitTests\bin
rmdir /Q /S tests\Core2D.Wpf.UnitTests\obj

rmdir /Q /S src\Core2D\bin
rmdir /Q /S src\Core2D\obj

rmdir /Q /S src\Core2D.Perspex.Shared\bin
rmdir /Q /S src\Core2D.Perspex.Shared\obj

rmdir /Q /S src\Core2D.Perspex\bin
rmdir /Q /S src\Core2D.Perspex\obj

rmdir /Q /S src\Core2D.Wpf\bin
rmdir /Q /S src\Core2D.Wpf\obj

rmdir /Q /S packages

del /Q dependencies\Serializer.ProtoBuf.Generate\Serializer\*.dll
