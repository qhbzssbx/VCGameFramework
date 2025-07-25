<explicit_instructions type="spec-driven-development.md"> <task name="Spec-Driven Development"> <task_objective>

总是使用中文回答，保持乐观的心态，鼓励我
Guide users through a complete spec-driven development process that transforms a rough feature idea into a fully implemented feature. The workflow takes a feature concept as input, processes it through four structured stages (requirements clarification, design document creation, implementation planning, and task execution), and outputs a complete implemented feature with supporting documentation files (requirements.md, design.md, tasks.md) in the current working directory along with all necessary code files and tests. This workflow leverages Cline's comprehensive toolset including file operations, terminal commands, code analysis, browser integration, and MCP tools to facilitate efficient development.

</task_objective>

<detailed_sequence_steps>

# Spec-Driven Development Process - Detailed Sequence of Steps

## Available Cline Tools for This Workflow

This workflow utilizes Cline's comprehensive toolset throughout all phases. The following tools are available:

### File Operations

- `write_to_file`: Create or overwrite files (requirements.md, design.md, tasks.md, code files)
- `read_file`: Read existing file contents for analysis and context
- `replace_in_file`: Make targeted edits to existing files during implementation
- `search_files`: Search files using regex patterns to understand codebase structure
- `list_files`: List directory contents to explore project structure

### Terminal Operations

- `execute_command`: Run CLI commands (npm install, build scripts, test runners, etc.)
- `list_code_definition_names`: List code definitions to understand existing architecture

### Code Analysis & Development

- `browser_action`: Test web applications, capture screenshots, inspect console logs
- `web_fetch`: Fetch external documentation and resources for research

### MCP Tools (if configured)

- `use_mcp_tool`: Use custom MCP server tools for specialized functionality
- `access_mcp_resource`: Access MCP server resources for additional context

### Interaction Tools

- `ask_followup_question`: Ask user for clarification during any phase
- `attempt_completion`: Present final results and completion status

- These tools will be used strategically throughout the workflow phases to ensure comprehensive development from requirements to implementation.
- Always use Interaction Tools when you need to ask the use questions.

## 1. Requirements Clarification

### 1.1 Initial Requirements Generation

1. Ask the user for their feature idea or concept if not already provided.
2. Generate an initial set of requirements in EARS format based on the feature idea WITHOUT asking sequential questions first.
3. Create a `.Spec/{feature_name}/requirements.md` file in the current working directory.
4. Format the requirements document with:
   - A clear introduction section that summarizes the feature
   - A hierarchical numbered list of requirements where each contains:
     - A user story in the format "As a [role], I want [feature], so that [benefit]"
     - A numbered list of acceptance criteria in EARS format (Easy Approach to Requirements Syntax)
5. Consider edge cases, user experience, technical constraints, and success criteria in the initial requirements.

### 1.2 Requirements Refinement Loop

1. After creating the initial requirements document, ask the user: "Do the requirements look good? If so, we can move on to the design."
2. If the user requests changes or does not explicitly approve:
   - Make modifications to the requirements document based on their feedback
   - Ask for explicit approval after every iteration of edits
3. Continue the feedback-revision cycle until explicit approval is received (such as "yes", "approved", "looks good", etc.)
4. Suggest specific areas where the requirements might need clarification or expansion if needed.
5. DO NOT proceed to the design document until receiving clear approval.

## 2. Design Document Creation

### 2.1 Research and Context Building

1. Identify areas where research is needed based on the feature requirements.
2. Conduct research and build up context in the conversation thread, use `web_fetch` if needed
3. Summarize key findings that will inform the feature design.
4. Cite sources and include relevant links in the conversation when applicable.

### 2.2 Design Document Generation

1. Create a `.Spec/{feature_name}/design.md` file in the current working directory.
2. Incorporate research findings directly into the design process.
3. Include the following sections in the design document:
   - Overview
   - Architecture
      - ALWAYS Represent diagrmas 
   - Key Flow 
      - ALWAYS Represent Flow Diagrams
   - Data Models
      - ALWAYS Represent Class Diagrams 
   - Components and Interfaces
      - ALWAYS Represent Class Diagrams
   - Error Handling
   - Performance Consideration
4. ALWAYS use UML Diagrams to represent designs (use Mermaid for diagrams).
5. Ensure the design addresses all feature requirements identified during the clarification process.
6. Highlight design decisions and their rationales.

### 2.3 Design Review Loop

1. After creating the design document, ask the user: "Does the design look good? If so, we can move on to the implementation plan."
2. If the user requests changes or does not explicitly approve:
   - Make modifications to the design document based on their feedback
   - Ask for explicit approval after every iteration of edits
3. Continue the feedback-revision cycle until explicit approval is received.
4. Offer to return to feature requirements clarification if gaps are identified during design.
5. DO NOT proceed to the implementation plan until receiving clear approval.

## 3. Implementation Planning

### 3.1 Task List Creation

1. Create a `.Spec/{feature_name}/tasks.md` file in the current working directory.
2. Convert the feature design into a series of prompts for code-generation that will implement each step in a test-driven manner.
3. Prioritize best practices, incremental progress, and early testing, ensuring no big jumps in complexity at any stage.
4. Ensure each task builds on the previous tasks, and ends with wiring things together.
5. Focus ONLY on tasks that involve writing, modifying, or testing code.

### 3.2 Task Formatting Requirements

1. Format the implementation plan as a numbered checkbox list with a maximum of two levels of hierarchy:
   - Top-level items (like epics) should be used only when needed
   - Sub-tasks should be numbered with decimal notation (e.g., 1.1, 1.2, 2.1)
   - Each item must be a checkbox
   - Simple structure is preferred
2. Ensure each task item includes:
   - A clear objective as the task description that involves writing, modifying, or testing code
   - Additional information as sub-bullets under the task
   - Specific references to requirements from the requirements document (referencing granular sub-requirements, not just user stories)
3. Ensure that the implementation plan is a series of discrete, manageable coding steps.
4. Ensure each task references specific requirements from the requirement document.
5. Assume that all context documents (feature requirements, design) will be available during implementation.
6. Ensure each step builds incrementally on previous steps.
7. Prioritize test-driven development where appropriate.
8. Ensure the plan covers all aspects of the design that can be implemented through code.
9. Sequence steps to validate core functionality early through code.
10. Ensure that all requirements are covered by the implementation tasks.

### 3.3 Task Content Constraints

1. ONLY include tasks that can be performed by a coding agent (writing code, creating tests, etc.).
2. DO NOT include tasks related to user testing, deployment, performance metrics gathering, or other non-coding activities.
3. Focus on code implementation tasks that can be executed within the development environment.
4. Ensure each task is actionable by a coding agent by following these guidelines:
   - Tasks should involve writing, modifying, or testing specific code components
   - Tasks should specify what files or components need to be created or modified
   - Tasks should be concrete enough that a coding agent can execute them without additional clarification
   - Tasks should focus on implementation details rather than high-level concepts
   - Tasks should be scoped to specific coding activities (e.g., "Implement X function" rather than "Support X feature")
5. Explicitly avoid including the following types of non-coding tasks:
   - User acceptance testing or user feedback gathering
   - Deployment to production or staging environments
   - Performance metrics gathering or analysis
   - Running the application to test end to end flows (automated tests are acceptable)
   - User training or documentation creation
   - Business process changes or organizational changes
   - Marketing or communication activities
   - Any task that cannot be completed through writing, modifying, or testing code

### 3.4 Implementation Plan Review Loop

1. After creating the tasks document, ask the user: "Do the tasks look good?"
2. If the user requests changes or does not explicitly approve:
   - Make modifications to the tasks document based on their feedback
   - Ask for explicit approval after every iteration of edits
3. Continue the feedback-revision cycle until explicit approval is received.
4. Offer to return to previous steps (requirements or design) if gaps are identified during implementation planning.
5. DO NOT consider the workflow complete until receiving clear approval.
6. Stop once the task document has been approved.

## 4. Task Execution

### 4.1 Pre-Execution Setup

1. ALWAYS ensure you have read the `.Spec/{feature_name}/requirements.md`, `.Spec/{feature_name}/design.md` and `.Spec/{feature_name}/tasks.md` files before executing any tasks.
2. Executing tasks without the requirements or design will lead to inaccurate implementations.

### 4.2 Task Execution Process

1. Look at the task details in the task list.
2. If the requested task has sub-tasks, always start with the sub tasks.
3. Only focus on ONE task at a time. Do not implement functionality for other tasks.
4. Verify your implementation against any requirements specified in the task or its details.
5. Once you complete the requested task, stop and let the user review. DO NOT just proceed to the next task in the list.

### 4.3 Task Selection and Recommendations

1. If the user doesn't specify which task they want to work on, look at the task list for that spec and make a recommendation on the next task to execute.
2. Remember, it is VERY IMPORTANT that you only execute one task at a time. Once you finish a task, stop. Don't automatically continue to the next task without the user asking you to do so.

### 4.4 Handling Task Questions

1. The user may ask questions about tasks without wanting to execute them. Don't always start executing tasks in cases like this.
2. For example, the user may want to know what the next task is for a particular feature. In this case, just provide the information and don't start any tasks.

## 5. Workflow Completion

1. This workflow is ONLY for creating design and planning artifacts and executing the implementation tasks.
2. DO NOT attempt to implement the feature as part of the planning workflow phases (steps 1-3).
3. Clearly communicate to the user that the planning workflow is complete once the design and planning artifacts are created.
4. Inform the user that they can begin executing tasks by referencing the tasks.md file and requesting specific task execution.
5. During task execution phase (step 4), continue until all tasks are completed or the user indicates they want to stop.

</detailed_sequence_steps>

</task>
</explicit_instructions>
