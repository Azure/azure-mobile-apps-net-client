﻿// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using DeviceTests.Shared.Helpers.Models;
using DeviceTests.Shared.TestPlatform;
using Microsoft.WindowsAzure.MobileServices;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DeviceTests.Shared.Tests
{
    public class Blogging_Tests : E2ETestBase
    {
        [Fact]
        public async Task PostComments()
        {
            IMobileServiceClient client = GetClient();
            IMobileServiceTable<BlogPost> postTable = client.GetTable<BlogPost>();
            IMobileServiceTable<BlogComment> commentTable = client.GetTable<BlogComment>();
            var userDefinedParameters = new Dictionary<string, string>() { { "state", "NY" }, { "tags", "#pizza #beer" } };

            // Add a few posts and a comment
            BlogPost post = new BlogPost { Title = "Windows 8" };
            await postTable.InsertAsync(post, userDefinedParameters);
            BlogPost highlight = new BlogPost { Title = "ZUMO" };
            await postTable.InsertAsync(highlight);
            await commentTable.InsertAsync(new BlogComment
            {
                BlogPostId = post.Id,
                UserName = "Anonymous",
                Text = "Beta runs great"
            });
            await commentTable.InsertAsync(new BlogComment
            {
                BlogPostId = highlight.Id,
                UserName = "Anonymous",
                Text = "Whooooo"
            });
            Assert.Equal(2, (await postTable.Where(p => p.Id == post.Id || p.Id == highlight.Id)
                                                .WithParameters(userDefinedParameters)
                                                .ToListAsync()).Count);

            // Navigate to the first post and add a comment
            BlogPost first = await postTable.LookupAsync(post.Id);
            Assert.Equal("Windows 8", first.Title);
            BlogComment opinion = new BlogComment { BlogPostId = first.Id, Text = "Can't wait" };
            await commentTable.InsertAsync(opinion);
            Assert.False(string.IsNullOrWhiteSpace(opinion.Id));
        }

        [Fact]
        public async Task PostExceptionMessageFromZumoRuntime()
        {
            IMobileServiceClient client = GetClient();
            IMobileServiceTable<BlogPost> postTable = client.GetTable<BlogPost>();

            // Delete a bog post that doesn't exist to get an exception generated by the
            // runtime.
            try
            {
                await postTable.DeleteAsync(new BlogPost() { CommentCount = 5, Id = "this_does_not_exist" });
            }
            catch (MobileServiceInvalidOperationException e)
            {
                bool isValid = e.Message.Contains("The request could not be completed.  (Not Found)") ||
                    e.Message.Contains("The item does not exist");
                Assert.True(isValid, "Unexpected error message: " + e.Message);
            }
        }

        [Fact]
        public async Task PostExceptionMessageFromUserScript()
        {
            IMobileServiceClient client = GetClient();
            IMobileServiceTable<BlogPost> postTable = client.GetTable<BlogPost>();

            // Insert a blog post that doesn't have a title; the user script will respond
            // with a 400 and an error message string in the response body.
            try
            {
                await postTable.InsertAsync(new BlogPost() { });
            }
            catch (MobileServiceInvalidOperationException e)
            {
                Assert.Equal("All blog posts must have a title.", e.Message);
            }
        }

        [Fact]
        public async Task PostCommentsWithDataContract()
        {
            IMobileServiceClient client = GetClient();
            IMobileServiceTable<DataContractBlogPost> postTable = client.GetTable<DataContractBlogPost>();

            // Add a few posts and a comment
            DataContractBlogPost post = new DataContractBlogPost() { Title = "How DataContracts Work" };
            await postTable.InsertAsync(post);
            DataContractBlogPost highlight = new DataContractBlogPost { Title = "Using the 'DataMember' attribute" };
            await postTable.InsertAsync(highlight);

            Assert.Equal(2, (await postTable.Where(p => p.Id == post.Id || p.Id == highlight.Id).ToListAsync()).Count);
        }
    }
}