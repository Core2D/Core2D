﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Core2D.Interfaces;
using Core2D.Renderer;
using Core2D.Shapes;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Core2D.Project
{
    /// <summary>
    /// Project input/output.
    /// </summary>
    public partial class XProject
    {
        /// <summary>
        /// Project ProtoBuf data entry name.
        /// </summary>
        public const string ProjectProtoBufEntryName = "Project.bin";

        /// <summary>
        /// Project Json data entry name.
        /// </summary>
        public const string ProjectJsonEntryName = "Project.json";

        /// <summary>
        /// Image Key prefix entry name.
        /// </summary>
        public const string ImageEntryNamePrefix = "Images\\";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileIO"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public static XProject Open(string path, IFileSystem fileIO, IStreamSerializer serializer)
        {
            using (var stream = fileIO.Open(path))
            {
                return Open(stream, fileIO, serializer);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="project"></param>
        /// <param name="path"></param>
        /// <param name="fileIO"></param>
        /// <param name="serializer"></param>
        public static void Save(XProject project, string path, IFileSystem fileIO, IStreamSerializer serializer)
        {
            using (var stream = fileIO.Create(path))
            {
                Save(project, stream, fileIO, serializer);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileIO"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public static XProject Open(Stream stream, IFileSystem fileIO, IStreamSerializer serializer)
        {
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                var projectEntry = archive.Entries.FirstOrDefault(e => e.FullName == ProjectProtoBufEntryName);
                var project = ReadProject(projectEntry, serializer);
                ReadImages(project, archive, fileIO);
                return project;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="project"></param>
        /// <param name="stream"></param>
        /// <param name="fileIO"></param>
        /// <param name="serializer"></param>
        public static void Save(XProject project, Stream stream, IFileSystem fileIO, IStreamSerializer serializer)
        {
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                var projectEntry = archive.CreateEntry(ProjectProtoBufEntryName);
                WriteProject(project, projectEntry, serializer);
                var keys = GetUsedKeys(project);
                WriteImages(project, keys, archive, fileIO);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileIO"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public static XProject Open(string path, IFileSystem fileIO, ITextSerializer serializer)
        {
            using (var stream = fileIO.Open(path))
            {
                return Open(stream, fileIO, serializer);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="project"></param>
        /// <param name="path"></param>
        /// <param name="fileIO"></param>
        /// <param name="serializer"></param>
        public static void Save(XProject project, string path, IFileSystem fileIO, ITextSerializer serializer)
        {
            using (var stream = fileIO.Create(path))
            {
                Save(project, stream, fileIO, serializer);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileIO"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public static XProject Open(Stream stream, IFileSystem fileIO, ITextSerializer serializer)
        {
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                var projectEntry = archive.Entries.FirstOrDefault(e => e.FullName == ProjectJsonEntryName);
                var project = ReadProject(projectEntry, fileIO, serializer);
                ReadImages(project, archive, fileIO);
                return project;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="project"></param>
        /// <param name="stream"></param>
        /// <param name="fileIO"></param>
        /// <param name="serializer"></param>
        public static void Save(XProject project, Stream stream, IFileSystem fileIO, ITextSerializer serializer)
        {
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                var projectEntry = archive.CreateEntry(ProjectJsonEntryName);
                WriteProject(project, projectEntry, fileIO, serializer);
                var keys = GetUsedKeys(project);
                WriteImages(project, keys, archive, fileIO);
            }
        }

        private static IEnumerable<string> GetUsedKeys(XProject project)
        {
            return XProject.GetAllShapes<XImage>(project).Select(i => i.Key).Distinct();
        }

        private static XProject ReadProject(ZipArchiveEntry projectEntry, IStreamSerializer serializer)
        {
            using (var entryStream = projectEntry.Open())
            {
                return serializer.Deserialize<XProject>(entryStream, null);
            }
        }

        private static void WriteProject(XProject project, ZipArchiveEntry projectEntry, IStreamSerializer serializer)
        {
            using (var entryStream = projectEntry.Open())
            {
                serializer.Serialize(entryStream, project);
            }
        }

        private static XProject ReadProject(ZipArchiveEntry projectEntry, IFileSystem fileIO, ITextSerializer serializer)
        {
            using (var entryStream = projectEntry.Open())
            {
                return serializer.Deserialize<XProject>(fileIO.ReadUtf8Text(entryStream));
            }
        }

        private static void WriteProject(XProject project, ZipArchiveEntry projectEntry, IFileSystem fileIO, ITextSerializer serializer)
        {
            using (var jsonStream = projectEntry.Open())
            {
                fileIO.WriteUtf8Text(jsonStream, serializer.Serialize(project));
            }
        }

        private static void ReadImages(IImageCache cache, ZipArchive archive, IFileSystem fileIO)
        {
            foreach (var entry in archive.Entries)
            {
                if (entry.FullName.StartsWith(ImageEntryNamePrefix))
                {
                    using (var entryStream = entry.Open())
                    {
                        var bytes = fileIO.ReadBinary(entryStream);
                        cache.AddImage(entry.FullName, bytes);
                    }
                }
            }
        }

        private static void WriteImages(IImageCache cache, IEnumerable<string> keys, ZipArchive archive, IFileSystem fileIO)
        {
            foreach (var key in keys)
            {
                var imageEntry = archive.CreateEntry(key);
                using (var imageStream = imageEntry.Open())
                {
                    fileIO.WriteBinary(imageStream, cache.GetImage(key));
                }
            }
        }
    }
}
