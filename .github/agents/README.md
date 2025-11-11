# GitHub Copilot Agents

This directory contains specialized GitHub Copilot agents configured for the FalconAPI project. Agents are specialized assistants that help with specific development tasks while following project conventions and best practices.

## Available Agents

### Code Review Recommendations Agent

**File**: `code-review-recommendations.agent.md`

**Purpose**: Applies recommended changes from code reviews while following ASP.NET Core, Entity Framework Core, and SignalR best practices.

**When to Use**:
- Applying feedback from pull request reviews
- Implementing suggestions from automated code analysis tools
- Addressing review comments from team members
- Refactoring code based on review recommendations

**Key Features**:
- Comprehensive ASP.NET Core best practices guidance
- Entity Framework Core optimization patterns
- SignalR real-time communication best practices
- Project-specific patterns (Repository, Service layers)
- Background processing with Workers
- Proper dependency injection
- Security and authorization patterns
- XML documentation standards
- Error handling and logging patterns

**Example Usage**:

1. When you receive a code review comment like:
   > "This endpoint should be async and include proper authorization"

2. Activate the Code Review Recommendations agent and provide the context:
   > "Apply the review recommendation to make the GetCompetition endpoint in CompetitionController async and add Admin/Teacher authorization"

3. The agent will:
   - Analyze the current code
   - Apply the changes following best practices
   - Add proper XML documentation
   - Ensure consistency with project patterns
   - Validate the changes

## How GitHub Copilot Agents Work

GitHub Copilot agents are specialized configurations that:

1. **Have Specific Expertise**: Each agent is trained with domain-specific knowledge and patterns
2. **Follow Project Conventions**: Agents understand and apply your project's coding standards
3. **Provide Consistent Results**: Agents ensure changes follow established patterns
4. **Include Context**: Agents have access to project-specific documentation and examples

## Agent File Format

Agents use a markdown format with YAML frontmatter:

```markdown
---
description: 'Brief description of the agent's purpose'
tools: ['editFiles', 'search', 'usages', 'codebase']
model: GPT-4.1
---

# Agent Name

Detailed instructions for the agent...
```

## Creating New Agents

To create a new agent for this project:

1. Create a new `.agent.md` file in this directory
2. Include YAML frontmatter with description, tools, and model
3. Write comprehensive instructions covering:
   - Core responsibilities
   - Best practices specific to the task
   - Project patterns to follow
   - Examples and code snippets
   - Quality checklist
4. Test the agent with various scenarios
5. Update this README with the new agent information

## Project-Specific Patterns

All agents in this project should be aware of:

### Architecture Layers
- **Controllers**: HTTP request handling, input validation
- **Services**: Business logic, transaction coordination
- **Repositories**: Data access abstraction
- **Hubs**: SignalR real-time communication
- **Workers**: Background task processing
- **Models**: Entity definitions and DTOs

### Coding Standards
- XML documentation for all public APIs
- Dependency injection via constructors
- Async/await for I/O operations
- Repository pattern for data access
- Service layer for business logic
- Request/Response DTOs for API boundaries

### Technology Stack
- .NET 8 / ASP.NET Core
- Entity Framework Core
- SignalR for real-time features
- MariaDB database
- JWT authentication
- Docker deployment

## Best Practices for Using Agents

1. **Be Specific**: Provide clear context about what needs to be changed
2. **Reference Files**: Mention specific files, classes, or methods when possible
3. **Include Requirements**: Specify any constraints or special requirements
4. **Review Changes**: Always review agent-generated changes before committing
5. **Iterative Approach**: Break large changes into smaller, manageable tasks
6. **Test Thoroughly**: Validate that changes work as expected

## Contributing

When adding or modifying agents:

1. Ensure comprehensive coverage of the agent's domain
2. Include practical examples and code snippets
3. Document all project-specific patterns
4. Test with real scenarios from the project
5. Update this README with usage guidance
6. Follow the established agent format

## Support

For questions or issues with agents:
- Review the agent's instructions file
- Check existing examples in the agent documentation
- Consult project documentation in the repository root
- Review similar implementations in the codebase

## Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core Documentation](https://docs.microsoft.com/ef/core)
- [SignalR Documentation](https://docs.microsoft.com/aspnet/core/signalr)
- [Project README](/README.md)
- [SignalR Hub Documentation](/SIGNALR_COMPETITION_HUB_DOCUMENTATION.md)
